using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.GameData;
using Dom5Edit.Props;
using Dom5Edit.Resolve;
using Dom5Editor.Data;
using Dom5Editor.Session;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// One entity's page: everything it has in game (ModResolver), grouped into the badge sections
    /// of its JSON config, type-specific panels (subclasses), and "other lines" for any value
    /// neither shows, so nothing it has is hidden. Every change goes through the session as an
    /// undoable edit; the page rebuilds after each one (an edit elsewhere, to a copy source, can
    /// change it too). docs/EDIT_FLOW.md, "GUI structure".
    /// </summary>
    public class EntityPageViewModel : INotifyPropertyChanged
    {
        private string? _error;

        public EntityPageViewModel(EditorSession session, EntityListItem item)
        {
            Session = session;
            Item = item;
            Session.Changed += OnSessionChanged;
            NavigateCommand = new RelayCommand<object>(p =>
            {
                if (p is ValueTuple<string, int> t) Navigate(t.Item1, t.Item2);
            });
            NavigateToCopyCommand = new RelayCommand(() =>
            {
                if (CopySourceId is int id) Session.Navigate(Type, id);
            });
            RemoveStructureCommand = new RelayCommand<StructureLine>(l => { if (l != null) RemoveLine(l.Property); });
            Resolved = Session.Resolve(Entity);
            Refresh();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditorSession Session { get; }
        public EntityListItem Item { get; }
        public IDEntity Entity => Item.Entity;
        public EntityType Type => Item.Type;

        /// <summary>What the entity is in game now.</summary>
        public ResolvedEntity Resolved { get; private set; }

        public void Detach() => Session.Changed -= OnSessionChanged;

        private bool _isActive = true;
        private bool _stale;

        /// <summary>Whether the page is on screen; a hidden page catches up when it's shown again.</summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (value && _stale)
                    Refresh();
            }
        }

        private void OnSessionChanged(IModEdit edit)
        {
            if (!_isActive)
            {
                _stale = true;
                return;
            }
            Refresh();
        }

        // ---- header ----

        public int ID => Entity.ID;

        public string Name
        {
            get => Resolved.Get(Command.NAME)?.Property is StringProperty s ? s.Value ?? "" : "";
            set
            {
                if (value != Name)
                    SetValue(Command.NAME, Quote(value));
            }
        }

        public virtual string DisplayName => !string.IsNullOrEmpty(Name) ? Name : Entity.HeaderName ?? $"#{ID}";

        public string SourceLabel => Item.IsNew ? "New in this mod"
            : !Item.IsModified ? "Vanilla"
            : Resolved.Vanilla == null && Entity.Selected ? $"Changed by this mod (the vanilla data has no {Plural(Type)} to show)"
            : "Vanilla, changed by this mod";

        /// <summary>Whether the type has a #name (poptypes and nametypes don't).</summary>
        public bool HasName => Entity.GetPropertyMap().ContainsKey(Command.NAME);

        public string Title => HasName ? DisplayName : Item.DisplayName;

        /// <summary>Why the last edit couldn't be made (null if it could).</summary>
        public string? Error
        {
            get => _error;
            private set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(_error);

        public bool HasDescription => Entity.GetPropertyMap().ContainsKey(Command.DESCR);

        /// <summary>Other long texts, each in its own box: an event's message, a nation's summary and brief, a spell's details.</summary>
        public ObservableCollection<LongText> LongTexts { get; } = new ObservableCollection<LongText>();

        /// <summary>The long texts, in order; an optional one (a spell's portent and cure) gets a box only when it has a text.</summary>
        private static readonly (Command Command, string Label, bool Optional)[] LongTextCommands =
        {
            (Command.MSG, "Message", false), (Command.SUMMARY, "Summary", false), (Command.BRIEF, "Brief", false),
            (Command.DETAILS, "Details", false), (Command.PORTENT, "Portent", true), (Command.CURE, "Cure", true),
        };

        /// <summary>Every command that can still be added, across the sections ("Section: command"), for the add box at the top.</summary>
        public List<AvailablePropertyItem> AllAvailable { get; } = new List<AvailablePropertyItem>();

        public ICommand AddAnyCommand => new RelayCommand<AvailablePropertyItem>(item =>
        {
            if (item == null)
                return;
            var section = Sections.Concat(Other != null ? new[] { Other } : Array.Empty<BadgeSectionViewModel>())
                .FirstOrDefault(sec => sec.Available.Any(a => a.Command == item.Command));
            section?.AddCommand.Execute(section.Available.First(a => a.Command == item.Command));
        });

        /// <summary>The description (#descr), or the vanilla one the editor shows when nothing sets it.</summary>
        public string Description
        {
            get => (Resolved.Get(Command.DESCR)?.Property ?? Resolved.Assets.GetValueOrDefault(Command.DESCR)) is StringProperty s ? s.Value ?? "" : "";
            set
            {
                if (value != Description)
                    SetValue(Command.DESCR, Quote(value));
            }
        }

        public string DescriptionTooltip => TextTooltip(Command.DESCR, Resolved.Get(Command.DESCR));

        /// <summary>A text box's tooltip: the command and where the text shown comes from.</summary>
        public string TextTooltip(Command c, ResolvedValue? value) => CommandName(c) + "\n" +
            (value != null ? SourceText(value)
             : Resolved.Assets.ContainsKey(c) ? $"The game's own text: shown, not saved. Editing it writes {CommandName(c)} into the mod."
             : "Not set");

        /// <summary>The entity's sprite (#spr1, an item's #spr), from the mod's folder or the vanilla assets; null if none.</summary>
        public System.Windows.Media.Imaging.BitmapSource? Sprite
        {
            get
            {
                var c = Entity.GetPropertyMap().ContainsKey(Command.SPR1) ? Command.SPR1 : Command.SPR;
                var p = Resolved.Get(c)?.Property ?? Resolved.Assets.GetValueOrDefault(c);
                return p is FilePathProperty f ? Sprites.SpriteLoader.Load(f.Value, Session.Mod.FullFilePath) : null;
            }
        }

        public bool HasSprite => Sprite != null;

        // ---- copy and clear lines ----

        /// <summary>The copy command this type has (#copystats, #copyweapon, ...), or null.</summary>
        public Command? CopyCommand => Type switch
        {
            EntityType.MONSTER => Command.COPYSTATS,
            EntityType.WEAPON => Command.COPYWEAPON,
            EntityType.ARMOR => Command.COPYARMOR,
            EntityType.ITEM => Command.COPYITEM,
            EntityType.SPELL => Command.COPYSPELL,
            EntityType.SITE => Command.COPYSITE,
            _ => null,
        };

        public bool HasCopy => CopyCommand != null;

        /// <summary>The entity this one copies (its own copy line), or null.</summary>
        public int? CopySourceId
        {
            get
            {
                var line = CopyCommand is Command c ? Resolved.Structure.LastOrDefault(p => p.Command == c) : null;
                if (line == null)
                    return null;
                var (id, _) = ReferenceOf(line, RefTypeName(Type));
                return id;
            }
            set
            {
                if (CopyCommand is not Command c || value == CopySourceId)
                    return;
                var line = Resolved.Structure.LastOrDefault(p => p.Command == c);
                if (value is int id && id > 0)
                    SetValue(c, id.ToString());
                else if (line != null)
                    RemoveLine(line);
            }
        }

        public string CopySourceName => CopySourceId is int id ? $"{NameOf(Type, id)} #{id}" : "(none)";

        public IEnumerable<ReferenceItem> CopyCandidates => Session.References(RefTypeName(Type));

        /// <summary>The entity's own copy and clear lines, in the order the game reads them.</summary>
        public ObservableCollection<StructureLine> Structure { get; } = new ObservableCollection<StructureLine>();

        public bool HasStructure => Structure.Count > 0;

        /// <summary>The clear commands the game reads for this type, to add one (saved before the entity's own lines).</summary>
        public IReadOnlyList<ReferenceItem> AddableClears => Entity.GetPropertyMap().Keys
            .Where(c => GameRules.IsClear(c) && GameCommandCatalog.IsRead(Type, c) == true && !Resolved.Structure.Any(p => p.Command == c))
            .Select(c => new ReferenceItem { ID = (int)c, DisplayName = CommandName(c) }).ToList();

        public bool HasClears => Entity.GetPropertyMap().Keys.Any(c => GameRules.IsClear(c) && GameCommandCatalog.IsRead(Type, c) == true);

        /// <summary>The add-clear picker's choice.</summary>
        public int? AddClearPick
        {
            get => null;
            set { if (value is int c && Enum.IsDefined(typeof(Command), c)) AddValue((Command)c, ""); }
        }

        public ICommand NavigateToCopyCommand { get; }
        public ICommand RemoveStructureCommand { get; }

        // ---- values ----

        public ObservableCollection<BadgeSectionViewModel> Sections { get; } = new ObservableCollection<BadgeSectionViewModel>();

        /// <summary>Values no section or panel shows (any command the game reads), editable as text.</summary>
        public BadgeSectionViewModel? Other { get; private set; }

        /// <summary>Type-specific panels (weapons, armor, magic, ...).</summary>
        public ObservableCollection<object> Panels { get; } = new ObservableCollection<object>();

        public IReadOnlyList<GameValue> GameValues => Resolved.GameValues;
        public bool HasGameValues => GameValues.Count > 0;

        /// <summary>Inherited abilities the entity removes (#fear 0).</summary>
        public IReadOnlyList<string> Removals => Resolved.Removals.Select(p => $"{CommandName(p.Command)} removed (set to 0)").ToList();
        public bool HasRemovals => Resolved.Removals.Count > 0;

        public ICommand NavigateCommand { get; }

        /// <summary>The entities that refer to this one (a weapon's monsters, a unit's nations), each a link.</summary>
        public ObservableCollection<UsageRow> UsedBy { get; } = new ObservableCollection<UsageRow>();

        public bool HasUsedBy => UsedBy.Count > 0;

        private const int UsedByShown = 24;
        private bool _showAllUsedBy;

        /// <summary>The first users, or all once asked for (a common weapon is used by hundreds).</summary>
        public IEnumerable<UsageRow> UsedByShownRows => _showAllUsedBy ? UsedBy : UsedBy.Take(UsedByShown);
        public bool HasMoreUsedBy => !_showAllUsedBy && UsedBy.Count > UsedByShown;
        public string ShowAllUsedByText => $"Show all {UsedBy.Count}";
        public ICommand ShowAllUsedByCommand => new RelayCommand(() =>
        {
            _showAllUsedBy = true;
            OnPropertyChanged(nameof(UsedByShownRows));
            OnPropertyChanged(nameof(HasMoreUsedBy));
        });

        private static bool _showFile;

        /// <summary>Whether the "in the file" box is open (remembered across pages; the text is only worked out while it is).</summary>
        public bool ShowFile
        {
            get => _showFile;
            set { _showFile = value; OnPropertyChanged(); OnPropertyChanged(nameof(FileText)); }
        }

        /// <summary>The entity's lines as the save writes them (the mod's own blocks), or a note when the mod has none.</summary>
        public string FileText
        {
            get
            {
                if (!_showFile)
                    return "";
                var own = Session.Editor.OwnEntity(Entity);
                if (own == null)
                    return "(not in the mod: vanilla as it is)";
                var lines = Dom5Edit.ModExporter.EntityLines(Session.Mod, own);
                return lines.Count > 0 ? string.Join("\n", lines) : "(not in the mod)";
            }
        }
        public string UsedByTitle { get; private set; } = "";

        private void BuildUsedBy()
        {
            UsedBy.Clear();
            if (ID <= 0)
                return;
            // one link per entity, with the commands it refers with
            var users = Session.Usage.UsedBy(Type, ID)
                .GroupBy(u => (u.Type, u.Id))
                .OrderBy(g => g.Key.Type).ThenBy(g => g.Key.Id)
                .ToList();
            const int shown = 200;
            foreach (var g in users.Take(shown))
            {
                var (type, id) = g.Key;
                var via = string.Join(", ", g.Select(u => CommandName(u.Via)).Distinct());
                UsedBy.Add(new UsageRow(type, id, NameOf(type, id), via, () => Session.Navigate(type, id)));
            }
            UsedByTitle = users.Count > shown ? $"USED BY ({users.Count}, first {shown} shown)" : $"USED BY ({users.Count})";
        }

        /// <summary>Rebuilds the page from what the entity is in game now.</summary>
        public void Refresh()
        {
            _stale = false;
            Resolved = Session.Resolve(Entity);
            var covered = new HashSet<Command> { Command.NAME, Command.DESCR };
            LongTexts.Clear();
            foreach (var (c, label, optional) in LongTextCommands)
                if (Entity.GetPropertyMap().ContainsKey(c) && ShowsLongText(c)
                    && (!optional || Resolved.Get(c) != null || Resolved.Assets.ContainsKey(c)))
                {
                    LongTexts.Add(new LongText(this, c, label));
                    covered.Add(c);
                }
            Structure.Clear();
            // (the last copy line is the copy picker's)
            var copyLine = CopyCommand is Command cc ? Resolved.Structure.LastOrDefault(p => p.Command == cc) : null;
            foreach (var p in Resolved.Structure.Where(p => !ReferenceEquals(p, copyLine)))
                Structure.Add(new StructureLine(p, $"{CommandName(p.Command)} {DisplayArguments(p)}".Trim(),
                    GameRules.IsCopy(p.Command) && ReferenceOf(p, RefTypeName(Type)) is var (id, _) && id > 0 ? $"{NameOf(Type, id)} #{id}" : ""));
            Panels.Clear();
            BuildPanels(covered);
            Sections.Clear();
            BuildSections(covered);
            Other = BuildOther(covered);
            AllAvailable.Clear();
            foreach (var sec in Sections.Append(Other))
                foreach (var a in sec.Available)
                    AllAvailable.Add(new AvailablePropertyItem
                    {
                        Command = a.Command, DisplayName = $"{a.DisplayName}  ({sec.Title.ToLowerInvariant()})",
                        DefaultValue = a.DefaultValue, IsReference = a.IsReference, ReferenceType = a.ReferenceType,
                        Tooltip = a.Tooltip,
                    });
            BuildUsedBy();
            BuildParts();
            OnPropertyChanged(string.Empty);
        }

        // ---- the page as parts (EntityPageView shows them as they scroll into view) ----

        /// <summary>The page's parts in order: its own (header, copies, ...), the panels, the badge sections, used by, ...</summary>
        public ObservableCollection<object> Parts { get; } = new ObservableCollection<object>();

        private readonly Dictionary<string, PagePart> _parts = new Dictionary<string, PagePart>();

        private PagePart Part(string kind) => _parts.TryGetValue(kind, out var p) ? p : _parts[kind] = new PagePart(kind, this);

        /// <summary>Puts the parts in place, replacing only the ones that changed (the scroll position stays).</summary>
        private void BuildParts()
        {
            var parts = new List<object> { Part("Header"), Part("Error") };
            if (HasDescription)
                parts.Add(Part("Description"));
            if (LongTexts.Count > 0)
                parts.Add(Part("LongTexts"));
            if (HasCopy || HasClears)
                parts.Add(Part("Copies"));
            parts.AddRange(Panels);
            if (ShowsAddBox)
                parts.Add(Part("AddBox"));
            parts.AddRange(Sections.Where(x => x.IsVisible));
            if (Other != null && Other.IsVisible)
                parts.Add(Other);
            if (HasRemovals)
                parts.Add(Part("Removals"));
            if (HasUsedBy)
                parts.Add(Part("UsedBy"));
            parts.Add(Part("File"));
            if (HasGameValues)
                parts.Add(Part("GameValues"));
            for (int i = 0; i < parts.Count; i++)
            {
                if (i >= Parts.Count)
                    Parts.Add(parts[i]);
                else if (!ReferenceEquals(Parts[i], parts[i]))
                    Parts[i] = parts[i];
            }
            while (Parts.Count > parts.Count)
                Parts.RemoveAt(Parts.Count - 1);
        }

        /// <summary>Whether the page has the add box for any command (an event adds lines in its own panels).</summary>
        public virtual bool ShowsAddBox => true;

        /// <summary>Whether the header shows the ID (events have none).</summary>
        public bool HasId => ID > 0;

        /// <summary>Whether a long text (#msg, #summary, ...) gets the generic box (an event's message has its own panel).</summary>
        protected virtual bool ShowsLongText(Command c) => true;

        /// <summary>Adds the type's panels; each adds the commands it shows to <paramref name="covered"/>.</summary>
        protected virtual void BuildPanels(HashSet<Command> covered) { }

        /// <summary>Section ids a panel draws instead (custom renderers); the generic sections skip them.</summary>
        protected virtual IEnumerable<string> PanelSections => Array.Empty<string>();

        /// <summary>The value the game uses for a command the entity doesn't set, when it isn't "none" (a monster's resource size is its size).</summary>
        protected virtual string? GameDefault(Command c) => DerivedValue(c);

        /// <summary>
        /// A value another line stores as a constant (the catalog's "values"): #teleport stores map
        /// move 100, #quadruped stores its item slots. Shown when nothing sets the command itself.
        /// </summary>
        protected string? DerivedValue(Command c)
        {
            var own = GameCommandCatalog.EffectOf(Type, c);
            if (own == null || own.Set.Count != 1)
                return null;
            var key = own.Set.First();
            for (int i = Resolved.Values.Count - 1; i >= 0; i--)
            {
                var e = GameCommandCatalog.EffectOf(Type, Resolved.Values[i].Command);
                if (e != null && e.Values.TryGetValue(key, out var v))
                    return v.ToString();
            }
            return null;
        }

        private void BuildSections(HashSet<Command> covered)
        {
            var config = BadgeConfigLoader.LoadConfig(ConfigName(Type));
            if (config == null)
                return;
            var skip = new HashSet<string>(PanelSections);
            foreach (var section in config.Sections)
            {
                // copy and clear lines are shown with the copy source, not as badges
                var commands = section.Commands
                    .Select(d => (Def: d, Ok: BadgeConfigLoader.TryGetCommand(d, out var c), Command: c))
                    .Where(x => x.Ok && Entity.GetPropertyMap().ContainsKey(x.Command) && !covered.Contains(x.Command)
                                && !GameRules.IsCopy(x.Command) && !GameRules.IsClear(x.Command))
                    .ToList();
                if (commands.Count == 0 || skip.Contains(section.Id) || section.HasCustomRenderer && !section.IsGridLayout)
                    continue;
                var vm = new BadgeSectionViewModel(this, section.Id, section.DisplayName ?? section.Id,
                    section.IsGridLayout, section.Columns, section.ReadOnly);
                foreach (var (def, _, c) in commands)
                {
                    if (!covered.Add(c))
                        continue;
                    string kind = KindOf(c, def.Type);
                    string? refType = kind == "ref" ? def.RefType : kind == "weaponref" ? "weapon" : null;
                    Brush? bg = def.HasColors ? new SolidColorBrush(BadgeConfigLoader.ParseColor(def.Color, Color.FromRgb(60, 60, 60))) : null;
                    Brush? border = def.HasColors ? new SolidColorBrush(BadgeConfigLoader.ParseColor(def.BorderColor, Color.FromRgb(80, 80, 80))) : null;
                    var values = Resolved.GetAll(c).ToList();
                    if (values.Count == 0)
                    {
                        var dflt = GameDefault(c) ?? (section.ShowDefaults && kind == "int" && def.Default.HasValue ? def.Default.Value.ToString() : null);
                        if (dflt != null && kind != "flag")
                        {
                            vm.AddBadge(null, c, def.Display, kind, def.Description, bg, border, null, dflt);
                        }
                        else if (dflt == "1" && kind == "flag")
                            vm.AddBadge(null, c, def.Display, kind, def.Description, bg, border, null);
                        continue;
                    }
                    bool many = refType != null || GameRules.IsRepeatable(Type, c) || GameRules.IsKeyedByFirstArgument(Type, c);
                    foreach (var v in many ? values : new List<ResolvedValue> { values[^1] })
                        vm.AddBadge(v, c, def.Display, kind, def.Description, bg, border, refType);
                }
                if (!section.ReadOnly)
                    foreach (var (def, _, c) in commands)
                    {
                        if (GameCommandCatalog.IsRead(Type, c) == false)
                            continue;
                        string kind = KindOf(c, def.Type);
                        bool many = kind == "ref" || kind == "weaponref" || GameRules.IsRepeatable(Type, c) || GameRules.IsKeyedByFirstArgument(Type, c);
                        if (many || !Resolved.Has(c))
                            vm.Available.Add(new AvailablePropertyItem
                            {
                                Command = c, DisplayName = def.Display,
                                DefaultValue = kind == "int" ? def.Default ?? 1 : null,
                                IsReference = kind == "ref" || kind == "weaponref",
                                ReferenceType = kind == "weaponref" ? "weapon" : def.RefType,
                                Tooltip = CommandHints.Tooltip(Type, c) ?? def.Description,
                            });
                    }
                Sections.Add(vm);
            }
        }

        /// <summary>
        /// How a badge shows a command: "flag" (no value), "ref"/"weaponref" (a picker, from the
        /// JSON type), else "int" or "value" (a text box). The line's own type decides flag or not.
        /// </summary>
        private string KindOf(Command c, string? jsonType)
        {
            var json = (jsonType ?? "flag").ToLowerInvariant();
            var sample = Entity.GetPropertyMap().TryGetValue(c, out var create) ? create() : null;
            if (sample is CommandProperty)
                return "flag";
            if (json == "ref" || json == "weaponref")
                return json;
            return json == "int" ? "int" : "value";
        }

        private BadgeSectionViewModel BuildOther(HashSet<Command> covered)
        {
            var vm = new BadgeSectionViewModel(this, "other", "OTHER LINES", false, 3, false);
            foreach (var v in Resolved.Values.Where(v => !covered.Contains(v.Command)))
                vm.AddBadge(v, v.Command, CommandName(v.Command).TrimStart('#'),
                    v.Property is CommandProperty ? "flag" : "text", null, null, null, null);
            // every other command the game reads for this type
            foreach (var (c, _) in Entity.GetPropertyMap())
            {
                if (covered.Contains(c) || GameRules.IsCopy(c) || GameRules.IsClear(c) || GameCommandCatalog.IsRead(Type, c) != true)
                    continue;
                if (Resolved.Has(c) && !GameRules.IsRepeatable(Type, c))
                    continue;
                vm.Available.Add(new AvailablePropertyItem { Command = c, DisplayName = CommandName(c).TrimStart('#'), Tooltip = CommandHints.Tooltip(Type, c) });
            }
            vm.Available.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));
            return vm;
        }

        // ---- edits (all through the session: undoable, and the page refreshes) ----

        protected void Edit(Func<ModEditor, IModEdit?> edit) => Error = Session.Edit(edit);

        /// <summary>Several changes as one undo step.</summary>
        public void EditRun(string description, Action<ModEditor, Transaction> body) =>
            Edit(ed => ed.Run(description, tx => body(ed, tx)));

        public void SetValue(Command c, string args) => Edit(ed => ed.Set(Entity, c, args));
        public void AddValue(Command c, string args) => Edit(ed => ed.Add(Entity, c, args));
        public void ChangeValue(ResolvedValue v, string args) => Edit(ed => ed.Change(Entity, v, args));
        public void RemoveValue(ResolvedValue v) => Edit(ed => ed.Remove(Entity, v));
        public void ResetValue(Command c) => Edit(ed => ed.Reset(Entity, c));

        /// <summary>Turns a flag on or off (off for an inherited one: the game's way, see ModEditor.Remove).</summary>
        public void SetFlag(Command c, bool on) => Edit(ed => ed.SetFlag(Entity, c, on));

        /// <summary>Moves one of the entity's lines to just before (or after) another (order matters in events).</summary>
        public void MoveLine(Property line, Property target, bool after) => Edit(ed => ed.MoveLine(Entity, line, target, after));

        /// <summary>Drops one of the mod's own values, so the entity has what it inherits again (not a removal: an inherited value stays).</summary>
        public void ResetLine(ResolvedValue v) => Edit(ed => ed.Run($"Reset {CommandName(v.Command)}", tx =>
        {
            var own = ed.OwnEntity(Entity) ?? throw new EditException("Not one of the mod's lines");
            tx.RemoveLine(own, v.Property);
        }));

        /// <summary>
        /// What the entity would have for a command without its own line, as text for a hint: the
        /// copy source's value when it copies, else vanilla's ("12 from vanilla"); null if neither has one.
        /// </summary>
        public string? InheritedText(Command c, string? key = null)
        {
            ResolvedValue? Find(ResolvedEntity r) => key == null ? r.Get(c) : r.GetAll(c).LastOrDefault(v => v.Selector == key);
            string Args(Property p)
            {
                var args = DisplayArguments(p);
                return key != null && args.StartsWith(key + " ") ? args.Substring(key.Length + 1) : args;
            }
            if (Resolved.CopyLine is Property copy && ReferenceOf(copy, RefTypeName(Type)).Id is int id && id > 0
                && Session.Mod.TryGet(Type, id, null, out var source))
                return Find(Session.Resolve(source)) is ResolvedValue sv ? $"{Args(sv.Property)} from {NameOf(Type, id)} #{id}" : null;
            if (Resolved.Vanilla != null)
            {
                // vanilla's own lines (resolving the vanilla entity would give the mod's state: same key)
                var line = Resolved.Vanilla.Properties.LastOrDefault(p => p.Command == c
                    && (key == null || ResolvedValue.ArgumentsOf(p).Split(' ')[0] == key));
                return line != null ? $"{Args(line)} from vanilla" : null;
            }
            return null;
        }

        /// <summary>Removes one of the entity's own lines (a copy or clear line).</summary>
        public void RemoveLine(Property line) => Edit(ed => ed.Run($"Remove {CommandName(line.Command)}", tx =>
        {
            var own = ed.OwnEntity(Entity) ?? throw new EditException("Not one of the mod's lines");
            tx.RemoveLine(own, line);
        }));

        /// <summary>Adds a command with a starting value: a flag as is, a number as its default (1 if none: 0 would remove an ability).</summary>
        public void AddDefault(Command c, int? defaultValue)
        {
            var map = Entity.GetPropertyMap();
            var sample = map.TryGetValue(c, out var create) ? create() : null;
            string args = sample switch
            {
                CommandProperty => "",
                StringProperty => "\"\"",
                IntIntProperty => "0 1",
                _ => (defaultValue ?? 1).ToString(),
            };
            AddValue(c, args);
        }

        // ---- formatting and lookups ----

        /// <summary>A line's arguments as shown in a badge (a string without its quotes).</summary>
        public string DisplayArguments(Property p)
        {
            var args = ResolvedValue.ArgumentsOf(p);
            return (p is StringProperty || p is FilePathProperty) && args.Length >= 2 && args[0] == '"' && args[^1] == '"' ? args[1..^1] : args;
        }

        /// <summary>Typed text as arguments for a command (quoted for a string).</summary>
        public string CommitArguments(Command c, string text)
        {
            text = text.Trim();
            var sample = Entity.GetPropertyMap().TryGetValue(c, out var create) ? create() : null;
            return (sample is StringProperty || sample is FilePathProperty) && !(text.StartsWith("\"") && text.EndsWith("\"")) ? Quote(text) : text;
        }

        private static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "'") + "\"";

        /// <summary>The entity a reference line points at: (ID, name).</summary>
        public (int Id, string Name) ReferenceOf(Property p, string refType)
        {
            int id = p switch
            {
                StringOrIDRef r => r.ID,
                MonsterOrMontagRef m when m.MonsterRef != null && m.MonsterRef.HasValue => m.MonsterRef.ID,
                MonsterOrMontagRef m when m.MontagRef != null && m.MontagRef.HasValue => m.MontagRef.ID,
                _ => int.TryParse(ResolvedValue.ArgumentsOf(p).Split(' ')[0], out var n) ? n : 0,
            };
            var type = BadgeConfigLoader.GetEntityTypeFromRefType(refType);
            string name = id < 0 && type == EntityType.MONSTER ? $"Montag {-id}" : type is EntityType t ? NameOf(t, id) : "";
            if (string.IsNullOrEmpty(name) && p is StringOrIDRef sr && sr.IsStringRef)
                name = sr.Name + " (not found)";
            if (string.IsNullOrEmpty(name))
                name = $"#{id}";
            return (id, name);
        }

        /// <summary>An entity's name in game, by type and ID ("" if there's none).</summary>
        public string NameOf(EntityType type, int id)
        {
            // (monster tags and other dependent entities aren't in the database)
            if (id <= 0 || !Session.Mod.Database.ContainsKey(type) || !Session.Mod.TryGet(type, id, null, out var e))
                return "";
            return Session.Resolve(e).Get(Command.NAME)?.Property is StringProperty s ? s.Value ?? "" : "";
        }

        /// <summary>Where a value comes from, for tooltips.</summary>
        public string SourceText(ResolvedValue v) => v.Source switch
        {
            ValueSource.Own => Session.Mod.IsFromFile(v.Property) ? $"Set by this mod (line {v.Property.LineNumber})" : "Set in this session",
            ValueSource.Vanilla => "From vanilla",
            _ => $"Copied from {NameOf(Type, v.CopiedFrom?.ID ?? 0)} #{v.CopiedFrom?.ID}" +
                 (v.Via != null ? v.Via.Source == ValueSource.Own ? " (its own line)" : v.Via.Source == ValueSource.Vanilla ? " (vanilla)" : " (which copies it)" : ""),
        };

        public void Navigate(string refType, int id)
        {
            if (BadgeConfigLoader.GetEntityTypeFromRefType(refType) is EntityType t)
                Session.Navigate(t, id);
        }

        public static string CommandName(Command c) => CommandsMap.TryGetString(c, out var s) ? s : c.ToString();

        public static string RefTypeName(EntityType t) => t.ToString().ToLowerInvariant();

        private static string Plural(EntityType t)
        {
            var s = t.ToString().ToLowerInvariant();
            return s.EndsWith("s") ? s + "es" : s.EndsWith("y") ? s[..^1] + "ies" : s + "s";
        }

        protected static string ConfigName(EntityType t) => t switch
        {
            EntityType.MERCENARY => "mercenary",
            _ => t.ToString().ToLowerInvariant(),
        };

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>A long text command (#msg, #summary, ...) in a multi-line box; leaving the box saves it.</summary>
    public sealed class LongText
    {
        private readonly EntityPageViewModel _page;

        public LongText(EntityPageViewModel page, Command command, string label)
        {
            _page = page;
            Command = command;
            Label = label;
            Value = page.Resolved.Get(command);
        }

        public Command Command { get; }
        public string Label { get; }
        public ResolvedValue? Value { get; }
        public bool IsInherited => Value == null || Value.Source != ValueSource.Own;
        public string Tooltip => _page.TextTooltip(Command, Value);

        /// <summary>The text, or the game's own one (a display asset) when nothing sets it.</summary>
        public string Text
        {
            get => (Value?.Property ?? _page.Resolved.Assets.GetValueOrDefault(Command)) is StringProperty s ? s.Value ?? "" : "";
            set
            {
                if (value == Text)
                    return;
                if (string.IsNullOrEmpty(value) && Value != null)
                    _page.RemoveValue(Value);
                else
                    _page.SetValue(Command, _page.CommitArguments(Command, value));
            }
        }
    }

    /// <summary>One of a page's own parts (its header, copies, used by, ...), shown with the page as its data.</summary>
    public sealed class PagePart
    {
        public PagePart(string kind, EntityPageViewModel page)
        {
            Kind = kind;
            Page = page;
        }

        /// <summary>Which part: Header, Error, Description, LongTexts, Copies, AddBox, Removals, UsedBy, File, GameValues.</summary>
        public string Kind { get; }
        public EntityPageViewModel Page { get; }
    }

    /// <summary>One entity that refers to the page's entity.</summary>
    public sealed class UsageRow
    {
        public UsageRow(EntityType type, int id, string name, string via, Action open)
        {
            Type = type;
            Id = id;
            Name = string.IsNullOrEmpty(name) ? $"#{id}" : name;
            Via = via;
            OpenCommand = new RelayCommand(open);
        }

        public EntityType Type { get; }
        public string TypeLabel => Type.ToString().ToLowerInvariant();
        public int Id { get; }
        public string Name { get; }
        public string Via { get; }
        public ICommand OpenCommand { get; }
    }

    /// <summary>One of an entity's own copy or clear lines, as the page lists them.</summary>
    public sealed class StructureLine
    {
        public StructureLine(Property property, string text, string target)
        {
            Property = property;
            Text = text;
            Target = target;
        }

        public Property Property { get; }
        public string Text { get; }
        public string Target { get; }
    }
}
