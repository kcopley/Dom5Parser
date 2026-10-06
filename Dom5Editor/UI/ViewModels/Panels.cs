using System.Collections.ObjectModel;
using System.Windows.Input;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Edit.Resolve;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>One entry of a list panel: a value the entity has, where it comes from, and a summary.</summary>
    public sealed class PanelRow
    {
        public PanelRow(ResolvedValue value, string text, string detail, string source, bool canRemove)
        {
            Value = value;
            Text = text;
            Detail = detail;
            Source = source;
            CanRemove = canRemove;
        }

        public ResolvedValue Value { get; }
        public string Text { get; }
        public string Detail { get; }
        /// <summary>Where it comes from (tooltip).</summary>
        public string Source { get; }
        public bool IsInherited => Value.Source != ValueSource.Own;
        public bool CanRemove { get; }
        /// <summary>For reference rows: the entity it points at.</summary>
        public int RefId { get; init; }
        /// <summary>An icon shown with the row (a GameIcon kind), or null.</summary>
        public string? Icon { get; init; }
        /// <summary>For a table panel: the row's values, one per column.</summary>
        public IReadOnlyList<string> Cells { get; init; } = Array.Empty<string>();
        private string _editText = "";

        /// <summary>For editable rows: the value as text; setting it (the box lost focus) commits the edit.</summary>
        public string EditText
        {
            get => _editText;
            set
            {
                if (_editText == value)
                    return;
                _editText = value;
                Edited?.Invoke(this);
            }
        }

        /// <summary>Called when EditText is set by the view.</summary>
        public Action<PanelRow>? Edited { get; set; }
    }

    /// <summary>
    /// A list of references the entity holds, one line each (a monster's weapons or armor; a nation's
    /// recruits): add one from a picker, remove one, open one. Removing or changing an inherited
    /// entry rewrites the list (ModEditor), which the panel says in its hint.
    /// </summary>
    public sealed class ReferenceListPanel
    {
        private readonly EntityPageViewModel _page;

        /// <summary>A column of a table panel: its header, icon, and the referenced entity's value it shows.</summary>
        public sealed record TableColumn(string Header, string? Icon, Func<Dom5Edit.Resolve.ResolvedEntity, string> Value, string Tooltip = "");

        public ReferenceListPanel(EntityPageViewModel page, string title, Command command, EntityType refType, Func<int, string>? detail = null,
            IReadOnlyList<TableColumn>? columns = null)
        {
            _page = page;
            Title = title;
            Command = command;
            RefType = refType;
            Columns = columns ?? Array.Empty<TableColumn>();
            foreach (var v in page.Resolved.GetAll(command))
            {
                var (id, name) = page.ReferenceOf(v.Property, EntityPageViewModel.RefTypeName(refType));
                IReadOnlyList<string> cells = Array.Empty<string>();
                if (Columns.Count > 0)
                    cells = id > 0 && page.Session.Mod.TryGet(refType, id, null, out var target)
                        ? Columns.Select(c => c.Value(page.Session.Resolve(target))).ToList()
                        : Columns.Select(_ => "").ToList();
                Rows.Add(new PanelRow(v, string.IsNullOrEmpty(name) ? $"#{id}" : name, detail?.Invoke(id) ?? "", page.SourceText(v), true) { RefId = id, Cells = cells });
            }
            Candidates = page.Session.References(refType);
            RemoveCommand = new RelayCommand<PanelRow>(r => { if (r != null) _page.RemoveValue(r.Value); });
            OpenCommand = new RelayCommand<PanelRow>(r => { if (r != null) _page.Session.Navigate(RefType, r.RefId); });
            CopyEditCommand = new RelayCommand<PanelRow>(CopyAndEdit);
            NewCommand = new RelayCommand(MakeNew);
        }

        public IReadOnlyList<TableColumn> Columns { get; }
        public bool IsTable => Columns.Count > 0;

        /// <summary>"New weapon" (armor, ...): makes a new one, gives it to this entity, and opens it.</summary>
        public bool CanMakeNew => RefType == EntityType.WEAPON || RefType == EntityType.ARMOR;
        public string NewLabel => $"+ New {RefType.ToString().ToLowerInvariant()}";
        public ICommand NewCommand { get; }

        private void MakeNew()
        {
            IDEntity? made = null;
            var owner = _page.Entity;
            var kind = RefType.ToString().ToLowerInvariant();
            _page.EditRun($"New {kind} for {_page.DisplayName}", (ed, tx) =>
            {
                made = tx.Create(RefType, $"{_page.DisplayName}'s {kind}");
                tx.Add(owner, Command, made.ID.ToString());
            });
            if (made != null && _page.Error == null)
                _page.Session.Navigate(RefType, made.ID);
        }

        /// <summary>The copy command of the referenced type, if it has one (#copyweapon, #copyarmor).</summary>
        private Command? CopyCommand => RefType switch
        {
            EntityType.WEAPON => Command.COPYWEAPON,
            EntityType.ARMOR => Command.COPYARMOR,
            EntityType.ITEM => Command.COPYITEM,
            EntityType.SPELL => Command.COPYSPELL,
            EntityType.MONSTER => Command.COPYSTATS,
            _ => null,
        };

        public bool CanCopyEdit => CopyCommand != null;

        /// <summary>
        /// The usual way to give a unit a changed weapon: a new weapon copying this one, used here in
        /// its place, then opened to edit (one undo step). Other users of the old one are untouched.
        /// </summary>
        private void CopyAndEdit(PanelRow? row)
        {
            if (row == null || CopyCommand is not Command copy)
                return;
            IDEntity? made = null;
            var owner = _page.Entity;
            _page.EditRun($"Copy {row.Text} for {_page.DisplayName}", (ed, tx) =>
            {
                made = tx.Create(RefType, $"{row.Text} ({_page.DisplayName})");
                tx.Set(made, copy, row.RefId.ToString()); // saved before its name: the copy doesn't overwrite it
                tx.Change(owner, row.Value, made.ID.ToString());
            });
            if (made != null && _page.Error == null)
                _page.Session.Navigate(RefType, made.ID);
        }

        public string Title { get; }
        public Command Command { get; }
        public EntityType RefType { get; }
        public ObservableCollection<PanelRow> Rows { get; } = new ObservableCollection<PanelRow>();
        public IReadOnlyList<ReferenceItem> Candidates { get; }
        public bool HasInherited => Rows.Any(r => r.IsInherited);
        public string Hint => HasInherited
            ? $"Grey entries are inherited. Removing one writes {ClearName} and adds the others back."
            : "";
        private string ClearName => Command switch
        {
            Command.WEAPON => "#clearweapons",
            Command.ARMOR => "#cleararmor",
            Command.ADDRECUNIT or Command.ADDRECCOM or Command.ADDFOREIGNUNIT or Command.ADDFOREIGNCOM => "#clearrec",
            _ => "a clear",
        };

        /// <summary>The add picker's choice: picking an entity adds it.</summary>
        public int? AddPick
        {
            get => null;
            set { if (value is int id && id != 0) _page.AddValue(Command, id.ToString()); }
        }

        public ICommand RemoveCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand CopyEditCommand { get; }
    }

    /// <summary>A magic path as a toggle (a random path's mask) or a button (add the path), with its icon.</summary>
    public sealed class PathToggle : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isOn;
        private readonly Action<PathToggle>? _changed;

        public PathToggle(int path, bool isOn, Action<PathToggle>? changed = null)
        {
            Path = path;
            _isOn = isOn;
            _changed = changed;
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public int Path { get; }
        public string Letter => MagicPanel.PathLetters[Path];
        public string Name => MagicPanel.PathNames[Path];
        public string Icon => "path:" + Letter;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn == value)
                    return;
                _isOn = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsOn)));
                _changed?.Invoke(this);
            }
        }
    }

    /// <summary>One #custommagic line: the paths it rolls among (toggles), its chance, where it comes from.</summary>
    public sealed class RandomPathRow
    {
        private readonly MagicPanel _panel;

        public RandomPathRow(MagicPanel panel, ResolvedValue? value, long mask, string chance, string source)
        {
            _panel = panel;
            Value = value;
            Source = source;
            _chance = chance;
            Toggles = Enumerable.Range(0, MagicPanel.PathNames.Length)
                .Select(i => new PathToggle(i, (mask >> (7 + i) & 1) != 0, OnToggled)).ToList();
        }

        public ResolvedValue? Value { get; }
        public string Source { get; }
        public bool IsInherited => Value != null && Value.Source != ValueSource.Own;
        public IReadOnlyList<PathToggle> Toggles { get; }
        public long Mask => Toggles.Where(t => t.IsOn).Aggregate(0L, (m, t) => m | 1L << (7 + t.Path));

        /// <summary>The paths as letters (FAWE), for the summary.</summary>
        public string Letters => string.Concat(Toggles.Where(t => t.IsOn).Select(t => t.Letter));

        /// <summary>A warning for a mask the game can't use: none, or holy alone (the manual: it crashes).</summary>
        public string Warning => Mask == 0 ? "Pick at least one path" : Mask == 1L << 16 ? "Holy alone crashes the game (manual): add another path" : "";
        public bool HasWarning => Warning.Length > 0;

        private string _chance;

        /// <summary>The chance in percent (1-100); setting it (the box lost focus) commits it.</summary>
        public string Chance
        {
            get => _chance;
            set
            {
                if (_chance == value)
                    return;
                _chance = value;
                if (Value != null)
                    _panel.Commit(this);
            }
        }

        private void OnToggled(PathToggle t)
        {
            if (Value != null && Mask != 0)
                _panel.Commit(this);
        }
    }

    /// <summary>
    /// A monster's magic: its paths (#magicskill path level; a new level for a path it has replaces
    /// the old one) as chips with their icons, added by clicking a path; and random paths
    /// (#custommagic mask chance, each line one more roll) as rows of path toggles with a chance.
    /// </summary>
    public sealed class MagicPanel
    {
        public static readonly string[] PathNames = { "Fire", "Air", "Water", "Earth", "Astral", "Death", "Nature", "Glamour", "Blood", "Holy" };
        public static readonly string[] PathLetters = { "F", "A", "W", "E", "S", "D", "N", "G", "B", "H" };

        private readonly EntityPageViewModel _page;

        public MagicPanel(EntityPageViewModel page)
        {
            _page = page;
            foreach (var v in page.Resolved.GetAll(Command.MAGICSKILL))
            {
                var p = v.Property as IntIntProperty;
                int path = p?.Value1 ?? -1, level = p?.Value2 ?? 0;
                string name = path >= 0 && path < PathNames.Length ? PathNames[path] : $"path {path}";
                var row = new PanelRow(v, name, level.ToString(), page.SourceText(v), true)
                {
                    RefId = path, EditText = level.ToString(), Icon = path >= 0 && path < PathLetters.Length ? "path:" + PathLetters[path] : null,
                };
                row.Edited = CommitLevel;
                Paths.Add(row);
            }
            foreach (var v in page.Resolved.GetAll(Command.CUSTOMMAGIC))
            {
                var args = ResolvedValue.ArgumentsOf(v.Property).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                long mask = args.Length > 0 && long.TryParse(args[0], out var m) ? m : 0;
                string chance = args.Length > 1 ? args[1] : "100";
                Random.Add(new RandomPathRow(this, v, mask, chance, page.SourceText(v)));
            }
            var have = new HashSet<int>(Paths.Select(r => r.RefId));
            AddablePaths = Enumerable.Range(0, PathNames.Length).Where(i => !have.Contains(i)).Select(i => new PathToggle(i, false)).ToList();
            NewRandom = new RandomPathRow(this, null, 0, "100", "");
            RemoveCommand = new RelayCommand<object>(r =>
            {
                if (r is PanelRow row) _page.RemoveValue(row.Value);
                else if (r is RandomPathRow rnd && rnd.Value != null) _page.RemoveValue(rnd.Value);
            });
            AddPathCommand = new RelayCommand<PathToggle>(t => { if (t != null) _page.SetValue(Command.MAGICSKILL, $"{t.Path} 1"); });
            AddRandomCommand = new RelayCommand(() =>
            {
                if (NewRandom.Mask != 0 && NewRandom.Mask != 1L << 16 && ChanceOf(NewRandom.Chance) is int c)
                    _page.AddValue(Command.CUSTOMMAGIC, $"{NewRandom.Mask} {c}");
            });
        }

        public ObservableCollection<PanelRow> Paths { get; } = new ObservableCollection<PanelRow>();
        public ObservableCollection<RandomPathRow> Random { get; } = new ObservableCollection<RandomPathRow>();
        public bool HasRandom => Random.Count > 0;

        /// <summary>The paths it doesn't have, as buttons: clicking one adds it at level 1.</summary>
        public IReadOnlyList<PathToggle> AddablePaths { get; }

        /// <summary>The random path being put together: toggle its paths, set the chance, add it.</summary>
        public RandomPathRow NewRandom { get; }

        public bool HasInherited => Paths.Any(r => r.IsInherited) || Random.Any(r => r.IsInherited);
        public string Hint => HasInherited ? "Grey entries are inherited. Removing one writes #clearmagic and adds the others back." : "";

        /// <summary>The magic as the game sums it up: "F3 S2, +1 random (FAWE) 100%".</summary>
        public string Summary
        {
            get
            {
                var fixedPaths = string.Join(" ", Paths.Select(p => (p.RefId >= 0 && p.RefId < PathLetters.Length ? PathLetters[p.RefId] : "?") + p.Detail));
                var random = string.Join(", ", Random.Select(r => $"+1 {r.Letters} {r.Chance}%"));
                return string.Join(", ", new[] { fixedPaths, random }.Where(x => x.Length > 0));
            }
        }

        public ICommand RemoveCommand { get; }
        public ICommand AddPathCommand { get; }
        public ICommand AddRandomCommand { get; }

        private void CommitLevel(PanelRow? row)
        {
            if (row == null || !int.TryParse(row.EditText, out var level) || level.ToString() == row.Detail)
                return;
            if (level <= 0)
                _page.RemoveValue(row.Value);
            else
                _page.ChangeValue(row.Value, $"{row.RefId} {level}");
        }

        internal void Commit(RandomPathRow row)
        {
            if (row.Value == null || row.Mask == 0 || ChanceOf(row.Chance) is not int chance)
                return;
            var args = $"{row.Mask} {chance}";
            if (args != row.Value.Arguments)
                _page.ChangeValue(row.Value, args);
        }

        private static int? ChanceOf(string text) => int.TryParse(text?.Trim().TrimEnd('%'), out var c) && c >= 1 && c <= 100 ? c : null;

        /// <summary>#custommagic path mask: bit 7 is fire, then air, water, earth, astral, death, nature, glamour, blood, holy.</summary>
        public static string MaskLetters(long mask)
        {
            var letters = Enumerable.Range(0, PathLetters.Length).Where(i => (mask >> (7 + i) & 1) != 0).Select(i => PathLetters[i]);
            var s = string.Concat(letters);
            return s.Length > 0 ? s : $"mask {mask}";
        }

        public static long? Mask(string letters)
        {
            long mask = 0;
            foreach (var ch in letters.ToUpperInvariant())
            {
                int i = Array.IndexOf(PathLetters, ch.ToString());
                if (i < 0)
                    return null;
                mask |= 1L << (7 + i);
            }
            return mask;
        }
    }

    /// <summary>
    /// A monster's item slots (#itemslots) as counts: hands, heads, body, feet, misc, and the bow
    /// and crown-only bits (the manual's "Item slot values" table). Other bits are kept as they are.
    /// With no #itemslots the game uses its default: 991750 (2 hands, bow, head, body, feet, 2 misc),
    /// or 786432 (2 misc) with #noitem; the body shape commands set their own.
    /// </summary>
    public sealed class ItemSlotsPanel : System.ComponentModel.INotifyPropertyChanged
    {
        private static readonly long[] HandBits = { 2, 4, 8, 16, 32, 64 };
        private static readonly long[] HeadBits = { 8192, 16384, 32768 };
        private static readonly long[] MiscBits = { 262144, 524288, 1048576, 2097152 };
        private const long Bow = 512, Body = 65536, Feet = 131072, CrownOnly = 16777216;

        private readonly EntityPageViewModel _page;
        private readonly long _mask;

        public ItemSlotsPanel(EntityPageViewModel page)
        {
            _page = page;
            // the last line that sets the slots (ability 182): #itemslots, or a body shape with its own
            var v = page.Resolved.Values.LastOrDefault(x => x.Command == Command.ITEMSLOTS
                || Dom5Edit.GameData.GameCommandCatalog.EffectOf(page.Type, x.Command)?.Values.ContainsKey("a182") == true);
            IsInherited = v == null || v.Source != ValueSource.Own || v.Command != Command.ITEMSLOTS;
            if (v != null && v.Command == Command.ITEMSLOTS && long.TryParse(v.Arguments, out var m))
            {
                _mask = m;
                Source = page.SourceText(v);
            }
            else if (v != null)
            {
                _mask = Dom5Edit.GameData.GameCommandCatalog.EffectOf(page.Type, v.Command)!.Values["a182"];
                Source = $"Set by {EntityPageViewModel.CommandName(v.Command)} (its slots); changing them adds #itemslots";
            }
            else
            {
                _mask = page.Resolved.Has(Command.NOITEM) ? 786432 : 991750;
                Source = "Not set: the game's default for the body (2 hands, bow, head, body, feet, 2 misc; #noitem: 2 misc)";
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public bool IsInherited { get; }
        public string Source { get; } = "";
        public long Mask => _mask;

        public int Hands { get => Count(HandBits); set => Commit(With(HandBits, value)); }
        public int Heads { get => Count(HeadBits); set => Commit(With(HeadBits, value)); }
        public int Misc { get => Count(MiscBits); set => Commit(With(MiscBits, value)); }
        public bool HasBody { get => (_mask & Body) != 0; set => Commit(Flag(Body, value)); }
        public bool HasFeet { get => (_mask & Feet) != 0; set => Commit(Flag(Feet, value)); }
        public bool HasBow { get => (_mask & Bow) != 0; set => Commit(Flag(Bow, value)); }
        public bool CrownsOnly { get => (_mask & CrownOnly) != 0; set => Commit(Flag(CrownOnly, value)); }

        private int Count(long[] bits) => bits.Count(b => (_mask & b) != 0);

        /// <summary>The mask with this many of the group's slots (the lowest bits first, as the table's values are).</summary>
        private long With(long[] bits, int count)
        {
            long m = _mask;
            for (int i = 0; i < bits.Length; i++)
                m = i < Math.Max(0, count) ? m | bits[i] : m & ~bits[i];
            return m;
        }

        private long Flag(long bit, bool on) => on ? _mask | bit : _mask & ~bit;

        private void Commit(long mask)
        {
            if (mask != _mask)
                _page.SetValue(Command.ITEMSLOTS, mask.ToString());
        }
    }

    /// <summary>
    /// A nametype's names (#addname), one per line. Names added at the end are saved as new
    /// #addname lines; any other change (a name removed, renamed or moved) as #clear and the whole
    /// list (the game only appends names).
    /// </summary>
    public sealed class NamesPanel
    {
        private readonly EntityPageViewModel _page;
        private readonly List<string> _names;

        public NamesPanel(EntityPageViewModel page)
        {
            _page = page;
            _names = page.Resolved.GetAll(Command.ADDNAME).Select(v => page.DisplayArguments(v.Property)).ToList();
        }

        public string Title => $"NAMES ({_names.Count})";

        /// <summary>A warning when the game's own names for this nametype aren't known (no vanilla nametype data).</summary>
        public string Hint => _page.Entity.Selected && _page.Resolved.Vanilla == null
            ? "These are only the names this mod adds: the game's own names for it aren't in the editor's data. Removing, renaming or reordering a name writes #clear, which removes the game's names too; adding names at the end doesn't."
            : "";

        public bool HasHint => Hint.Length > 0;

        public string Text
        {
            get => string.Join("\n", _names);
            set
            {
                var names = (value ?? "").Replace("\r", "").Split('\n').Select(n => n.Trim()).Where(n => n.Length > 0).ToList();
                if (names.SequenceEqual(_names))
                    return;
                var entity = _page.Entity;
                if (names.Count > _names.Count && names.Take(_names.Count).SequenceEqual(_names))
                {
                    var added = names.Skip(_names.Count).ToList();
                    _page.EditRun($"Add {added.Count} names", (ed, tx) =>
                    {
                        foreach (var n in added)
                            tx.Add(entity, Command.ADDNAME, "\"" + n.Replace("\"", "'") + "\"");
                    });
                    return;
                }
                _page.EditRun("Rewrite the names", (ed, tx) =>
                {
                    var own = tx.Editable(entity);
                    foreach (var p in own.Properties.Where(p => p.Command == Command.ADDNAME || p.Command == Command.CLEAR).ToList())
                        tx.RemoveLine(own, p);
                    tx.Add(own, Command.CLEAR, "");
                    foreach (var n in names)
                        tx.Add(own, Command.ADDNAME, "\"" + n.Replace("\"", "'") + "\"");
                });
            }
        }
    }
}
