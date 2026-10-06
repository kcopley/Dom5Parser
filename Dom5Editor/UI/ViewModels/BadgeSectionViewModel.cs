using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.GameData;
using Dom5Edit.Props;
using Dom5Edit.Resolve;
using Dom5Editor.Data;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// A group of badges on an entity page: one JSON badge section (Dom5Editor/Data/*_badges.json),
    /// or the page's "other lines" (values no section shows). Each badge is one value the entity has
    /// in game (ModResolver), styled by where it comes from; typing in it, removing it, adding one or
    /// picking a reference makes the edit through the page (and so the session, with undo).
    /// </summary>
    public sealed class BadgeSectionViewModel
    {
        private readonly EntityPageViewModel _page;

        public BadgeSectionViewModel(EntityPageViewModel page, string id, string title, bool isGrid, int columns, bool readOnly)
        {
            _page = page;
            Id = id;
            Title = title;
            IsGrid = isGrid;
            Columns = columns;
            ReadOnly = readOnly;
            RemoveCommand = new RelayCommand<PropertyItem>(b => { if (b?.Tag is ResolvedValue v) _page.RemoveValue(v); });
            AddCommand = new RelayCommand<AvailablePropertyItem>(Add);
            NavigateCommand = new RelayCommand<object>(Navigate);
            ReferenceChangedCommand = new RelayCommand<object>(ReferenceChanged);
        }

        public string Id { get; }
        public string Title { get; }
        public bool IsGrid { get; }
        public int Columns { get; }
        public bool ReadOnly { get; }

        public ObservableCollection<PropertyItem> Badges { get; } = new ObservableCollection<PropertyItem>();

        /// <summary>Commands that can be added here (not set yet, or repeatable), as the add box lists them.</summary>
        public List<AvailablePropertyItem> Available { get; } = new List<AvailablePropertyItem>();

        public bool CanAdd => !ReadOnly && Available.Count > 0;
        public bool HasBadges => Badges.Count > 0;

        /// <summary>Shown when it has values (or is a grid of stats); the add box at the top of the page adds to the others.</summary>
        public bool IsVisible => HasBadges || IsGrid;

        public ICommand RemoveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand ReferenceChangedCommand { get; }

        /// <summary>A badge for one value, wired to edit it.</summary>
        internal PropertyItem AddBadge(ResolvedValue? value, Command command, string label, string kind, string? tooltip, Brush? background, Brush? border, string? refType, string? defaultText = null)
        {
            var r = _page.Resolved;
            var badge = new PropertyItem { Command = command, DisplayName = label, Tag = value, IconKind = IconOf(command) };
            if (background != null) badge.Background = background;
            if (border != null) badge.BorderBrush = border;
            bool own = value != null && r.IsEditableInPlace(value);
            badge.IsModified = value?.Source == ValueSource.Own;
            badge.IsSessionEdit = value != null && value.Source == ValueSource.Own && !_page.Session.Mod.IsFromFile(value.Property);
            badge.IsInherited = value == null || value.Source != ValueSource.Own;
            badge.IsNotReadByGame = GameCommandCatalog.IsRead(_page.Type, command) == false;
            badge.CanRemove = !ReadOnly && value != null && (own || Transaction.CanRemoveInherited(_page.Type, command));
            badge.Tooltip = Tooltip(tooltip, value, command);

            if (refType != null && value != null)
            {
                var (id, name) = _page.ReferenceOf(value.Property, refType);
                badge.IsReference = true;
                badge.ReferenceType = refType;
                badge.ReferenceId = id;
                badge.ReferenceName = name;
                if (!ReadOnly && !badge.IsLocked)
                    badge.AvailableReferences = _page.Session.References(refType);
                badge.ReferenceSelectionChanged += (s, e) => _page.ChangeValue(value, e.NewId.ToString());
            }
            else if (kind != "flag")
            {
                badge.HasValue = true;
                // set before wiring the commit, so it doesn't count as an edit
                badge.Value = value != null ? _page.DisplayArguments(value.Property) : defaultText ?? "";
                var note = CommandHints.ValueNote(_page.Type, command, badge.Value ?? "");
                badge.ValueNote = note.Length > 0 ? note : null;
                badge.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != nameof(PropertyItem.Value))
                        return;
                    var text = badge.Value ?? "";
                    if (value != null)
                        _page.ChangeValue(value, _page.CommitArguments(command, text));
                    else
                        _page.SetValue(command, _page.CommitArguments(command, text));
                };
            }
            Badges.Add(badge);
            return badge;
        }

        /// <summary>Icons for abilities modders know at a glance (resistances, flying, stealth, ...).</summary>
        private static readonly Dictionary<Command, string> Icons = new Dictionary<Command, string>
        {
            [Command.FIRERES] = "path:F", [Command.COLDRES] = "cold", [Command.SHOCKRES] = "shock", [Command.POISONRES] = "poison",
            [Command.FLYING] = "wing", [Command.FLOAT] = "wing", [Command.AQUATIC] = "water", [Command.AMPHIBIAN] = "water",
            [Command.STEALTHY] = "stealth", [Command.ETHEREAL] = "ghost", [Command.REGENERATION] = "regen",
            [Command.FEAR] = "fear", [Command.AWE] = "awe", [Command.ANIMALAWE] = "awe",
            [Command.DARKVISION] = "sight", [Command.SPIRITSIGHT] = "sight", [Command.TRUESIGHT] = "sight",
            [Command.HOLY] = "path:H", [Command.BLUNTRES] = "def", [Command.PIERCERES] = "def", [Command.SLASHRES] = "def",
            [Command.BERSERK] = "att", [Command.MAGICBOOST] = "mr",
        };

        private static string? IconOf(Command c) => Icons.TryGetValue(c, out var k) ? k : null;

        private string Tooltip(string? description, ResolvedValue? value, Command command)
        {
            var lines = new List<string>();
            // the manual's description (the badge config's own text is the fallback)
            if (CommandHints.Tooltip(_page.Type, command) is string hint)
                lines.Add(hint + "\n");
            else if (!string.IsNullOrEmpty(description))
                lines.Add(description!);
            var cmd = CommandsMap.TryGetString(command, out var s) ? s : command.ToString();
            if (value == null)
                lines.Add($"{cmd}: not set; shown is the game's default. Typing a value sets it.");
            else
            {
                lines.Add(value.Property.ToExportString());
                lines.Add(_page.SourceText(value));
            }
            if (GameCommandCatalog.IsRead(_page.Type, command) == false)
                lines.Add($"The game doesn't read {cmd} for this type: it changes nothing in game.");
            return string.Join("\n", lines);
        }

        private void Add(AvailablePropertyItem? item)
        {
            if (item == null)
                return;
            if (item.IsReference)
            {
                // a reference starts empty: the picker opens on the new badge, the pick adds it
                var pending = new PropertyItem
                {
                    Command = item.Command, DisplayName = item.DisplayName, IsReference = true,
                    ReferenceType = item.ReferenceType, ReferenceId = 0, ReferenceName = "(pick)",
                    CanRemove = false, AvailableReferences = _page.Session.References(item.ReferenceType),
                };
                pending.ReferenceSelectionChanged += (s, e) => _page.AddValue(item.Command, e.NewId.ToString());
                Badges.Add(pending);
                return;
            }
            _page.AddDefault(item.Command, item.DefaultValue);
        }

        private void Navigate(object? param)
        {
            if (param is ValueTuple<string, int> t)
                _page.Navigate(t.Item1, t.Item2);
            else if (param is PropertyItem b && b.IsReference)
                _page.Navigate(b.ReferenceType, b.ReferenceId);
        }

        private void ReferenceChanged(object? param)
        {
            // CompactBadge reports a new pick as (badge, old, new, name); the badge's own event handles it
            if (param is ValueTuple<PropertyItem, int, int, string> t)
                t.Item1.OnReferenceChanged(t.Item2, t.Item3, t.Item4);
        }
    }
}
