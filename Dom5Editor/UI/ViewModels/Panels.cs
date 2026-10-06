using System.Collections.ObjectModel;
using System.Windows.Input;
using Dom5Edit.Commands;
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

        public ReferenceListPanel(EntityPageViewModel page, string title, Command command, EntityType refType, Func<int, string>? detail = null)
        {
            _page = page;
            Title = title;
            Command = command;
            RefType = refType;
            foreach (var v in page.Resolved.GetAll(command))
            {
                var (id, name) = page.ReferenceOf(v.Property, EntityPageViewModel.RefTypeName(refType));
                Rows.Add(new PanelRow(v, string.IsNullOrEmpty(name) ? $"#{id}" : name, detail?.Invoke(id) ?? "", page.SourceText(v), true) { RefId = id });
            }
            Candidates = page.Session.References(refType);
            RemoveCommand = new RelayCommand<PanelRow>(r => { if (r != null) _page.RemoveValue(r.Value); });
            OpenCommand = new RelayCommand<PanelRow>(r => { if (r != null) _page.Session.Navigate(RefType, r.RefId); });
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
        private string ClearName => Command == Command.WEAPON ? "#clearweapons" : Command == Command.ARMOR ? "#cleararmor" : "a clear";

        /// <summary>The add picker's choice: picking an entity adds it.</summary>
        public int? AddPick
        {
            get => null;
            set { if (value is int id && id != 0) _page.AddValue(Command, id.ToString()); }
        }

        public ICommand RemoveCommand { get; }
        public ICommand OpenCommand { get; }
    }

    /// <summary>
    /// A monster's magic: its paths (#magicskill path level; a new level for a path it has replaces
    /// the old one) and random paths (#custommagic mask chance, each line one more roll).
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
                var row = new PanelRow(v, name, level.ToString(), page.SourceText(v), true) { RefId = path, EditText = level.ToString() };
                row.Edited = CommitLevel;
                Paths.Add(row);
            }
            foreach (var v in page.Resolved.GetAll(Command.CUSTOMMAGIC))
            {
                var args = ResolvedValue.ArgumentsOf(v.Property).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                long mask = args.Length > 0 && long.TryParse(args[0], out var m) ? m : 0;
                string chance = args.Length > 1 ? args[1] : "100";
                var row = new PanelRow(v, MaskLetters(mask), chance + "%", page.SourceText(v), true) { EditText = chance };
                row.Edited = CommitChance;
                Random.Add(row);
            }
            var have = new HashSet<int>(Paths.Select(r => r.RefId));
            AddablePaths = Enumerable.Range(0, PathNames.Length).Where(i => !have.Contains(i))
                .Select(i => new ReferenceItem { ID = i, DisplayName = PathNames[i] }).ToList();
            RemoveCommand = new RelayCommand<PanelRow>(r => { if (r != null) _page.RemoveValue(r.Value); });
            AddRandomCommand = new RelayCommand<object>(p =>
            {
                // a new roll among the elements at 100%; the mask is edited by letters below
                if (p is string letters && Mask(letters) is long mask && mask > 0)
                    _page.AddValue(Command.CUSTOMMAGIC, $"{mask} 100");
            });
        }

        public ObservableCollection<PanelRow> Paths { get; } = new ObservableCollection<PanelRow>();
        public ObservableCollection<PanelRow> Random { get; } = new ObservableCollection<PanelRow>();
        public IReadOnlyList<ReferenceItem> AddablePaths { get; }
        public bool HasInherited => Paths.Concat(Random).Any(r => r.IsInherited);
        public string Hint => HasInherited ? "Grey entries are inherited. Removing one writes #clearmagic and adds the others back." : "";

        /// <summary>The add-path picker's choice: picking a path adds it at level 1.</summary>
        public int? AddPathPick
        {
            get => null;
            set { if (value is int path && path >= 0) _page.SetValue(Command.MAGICSKILL, $"{path} 1"); }
        }

        public ICommand RemoveCommand { get; }
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

        private void CommitChance(PanelRow? row)
        {
            if (row == null || !int.TryParse(row.EditText, out var chance) || chance + "%" == row.Detail)
                return;
            var mask = ResolvedValue.ArgumentsOf(row.Value.Property).Split(' ')[0];
            _page.ChangeValue(row.Value, $"{mask} {chance}");
        }

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
}
