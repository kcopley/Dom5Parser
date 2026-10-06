using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.Validation;
using Dom5Editor.Session;
using Dom5Editor.UI.ViewModels;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// The main window: the open mod (EditorSession), a tab per entity type plus the mod's header,
    /// undo/redo, saving, and back/forward through the entities visited.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly (EntityType Type, string Title)[] TabTypes =
        {
            (EntityType.MONSTER, "Monsters"), (EntityType.WEAPON, "Weapons"), (EntityType.ARMOR, "Armor"),
            (EntityType.SPELL, "Spells"), (EntityType.ITEM, "Items"), (EntityType.SITE, "Sites"),
            (EntityType.NATION, "Nations"), (EntityType.EVENT, "Events"), (EntityType.MERCENARY, "Mercenaries"),
            (EntityType.POPTYPE, "Poptypes"), (EntityType.NAMETYPE, "Nametypes"), (EntityType.BLESS, "Blesses"),
            (EntityType.TEMPLATE, "Templates"),
        };

        private EditorSession? _session;
        private object? _selectedTab;
        // (without the game the editor still works: only the game's texts, sprites and messages are missing)
        private string _statusMessage = Dom5Edit.Events.GameInstall.Exe() == null
            ? "Dominions 6 wasn't found: the game's texts, sprites and event messages aren't shown (▾ next to Load: Dominions 6 folder...)"
            : "Ready";
        private readonly List<(EntityTypeTab Tab, EntityListItem Item)> _back = new();
        private readonly List<(EntityTypeTab Tab, EntityListItem Item)> _forward = new();
        private (EntityTypeTab Tab, EntityListItem Item)? _current;
        private bool _travelling;

        public event PropertyChangedEventHandler? PropertyChanged;

        public EditorSession? Session => _session;
        public bool HasMod => _session != null;
        public string? CurrentFilePath => _session?.FilePath;
        public string ModName => _session?.Mod.ModName ?? "No mod loaded";

        /// <summary>The mod header tab, then one tab per entity type.</summary>
        public ObservableCollection<object> Tabs { get; } = new ObservableCollection<object>();

        public object? SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                // only the page on screen refreshes after each edit; the others when shown again
                foreach (var tab in Tabs.OfType<EntityTypeTab>())
                    if (tab.Page != null)
                        tab.Page.IsActive = ReferenceEquals(tab, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPage));
            }
        }

        /// <summary>The page shown on the selected tab, if any.</summary>
        public EntityPageViewModel? SelectedPage => (SelectedTab as EntityTypeTab)?.Page;

        public EntityTypeTab? TabOf(EntityType type) => Tabs.OfType<EntityTypeTab>().FirstOrDefault(t => t.Type == type);

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool CanUndo => _session?.History.CanUndo == true;
        public bool CanRedo => _session?.History.CanRedo == true;
        public bool IsDirty => _session?.History.IsDirty == true;
        public string? UndoDescription => _session?.History.UndoDescription is string d ? "Undo: " + d : null;
        public string? RedoDescription => _session?.History.RedoDescription is string d ? "Redo: " + d : null;
        public string UndoTip => (UndoDescription ?? "Nothing to undo") + " (Ctrl+Z)";
        public string RedoTip => (RedoDescription ?? "Nothing to redo") + " (Ctrl+Y)";
        public bool CanGoBack => _back.Count > 0;
        public bool CanGoForward => _forward.Count > 0;

        public int EntityCount => _session?.Mod.Database.Values.Sum(s => s.GetFullList().Count) ?? 0;

        // ---- mod ----

        public void CreateNewMod() => Open(EditorSession.New(), "Created a new mod");

        public void LoadMod(string filePath)
        {
            var session = EditorSession.Load(filePath);
            int issues = session.Mod.ParseIssues.Count;
            Open(session, $"Loaded {System.IO.Path.GetFileName(filePath)}" +
                (issues > 0 ? $": {issues} notes from reading it (commands the game ignores, duplicates, ...): Validate lists them" : ""));
        }

        public void SaveMod(string filePath)
        {
            if (_session == null)
                return;
            _session.Save(filePath);
            StatusMessage = $"Saved {System.IO.Path.GetFileName(filePath)}";
            OnPropertyChanged(nameof(CurrentFilePath));
        }

        private void Open(EditorSession session, string status)
        {
            _session = session;
            _back.Clear();
            _forward.Clear();
            _current = null;
            _jumpTargets = null;
            session.History.Changed += OnHistoryChanged;
            session.Changed += OnSessionChanged;
            session.NavigationRequested += NavigateToEntity;
            session.EntityNavigationRequested += NavigateToEntity;
            Tabs.Clear();
            Tabs.Add(new ModInfoViewModel(session, this));
            foreach (var (type, title) in TabTypes)
            {
                var tab = new EntityTypeTab(session, type, title);
                tab.Selected += OnSelected;
                tab.Status += message => StatusMessage = message;
                tab.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != nameof(EntityTypeTab.Page))
                        return;
                    if (tab.Page != null)
                        tab.Page.IsActive = ReferenceEquals(tab, SelectedTab);
                    OnPropertyChanged(nameof(SelectedPage));
                };
                Tabs.Add(tab);
            }
            SelectedTab = Tabs[1];
            StatusMessage = status;
            OnPropertyChanged(string.Empty);
        }

        private void OnSessionChanged(IModEdit edit)
        {
            if (edit.Entities.Count > 0)
                _jumpTargets = null; // names or entities may have changed; rebuilt on next use
            foreach (var tab in Tabs.OfType<EntityTypeTab>())
                tab.OnChanged(edit);
            OnPropertyChanged(nameof(EntityCount));
            OnPropertyChanged(nameof(ModName));
        }

        private void OnHistoryChanged()
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(UndoDescription));
            OnPropertyChanged(nameof(RedoDescription));
            OnPropertyChanged(nameof(UndoTip));
            OnPropertyChanged(nameof(RedoTip));
        }

        public void Undo()
        {
            var edit = _session?.Undo();
            if (edit != null)
                StatusMessage = "Undone: " + edit.Description;
        }

        public void Redo()
        {
            var edit = _session?.Redo();
            if (edit != null)
                StatusMessage = "Redone: " + edit.Description;
        }

        public ValidationResult? Validate()
        {
            if (_session == null)
                return null;
            var results = new ModValidator().ValidateWithSummary(_session.Mod);
            StatusMessage = $"Validation: {results.ErrorCount} errors, {results.WarningCount} warnings";
            return results;
        }

        // ---- navigation ----

        private List<Controls.ReferenceItem>? _jumpTargets;

        /// <summary>Every entity of every type, for the "Go to" box: name and type, ID; built when first used.</summary>
        public IReadOnlyList<Controls.ReferenceItem>? JumpTargets => _jumpTargets;

        public void EnsureJumpTargets()
        {
            if (_session == null || _jumpTargets != null)
                return;
            _jumpTargets = new List<Controls.ReferenceItem>();
            foreach (var (type, title) in TabTypes)
                foreach (var r in _session.References(type))
                    _jumpTargets.Add(new Controls.ReferenceItem { ID = r.ID, DisplayName = $"{r.DisplayName}  ({title.TrimEnd('s').ToLowerInvariant()})", Tag = (type, r.ID) });
            OnPropertyChanged(nameof(JumpTargets));
        }

        /// <summary>Goes to a "Go to" box pick.</summary>
        public void JumpTo(Controls.ReferenceItem? item)
        {
            if (item?.Tag is ValueTuple<EntityType, int> t)
                NavigateToEntity(t.Item1, t.Item2);
        }

        /// <summary>Shows an entity: its type's tab, selected in the list.</summary>
        public void NavigateToEntity(EntityType type, int id)
        {
            var tab = TabOf(type);
            if (tab == null)
            {
                StatusMessage = $"{type} #{id} has no tab";
                return;
            }
            SelectedTab = tab;
            if (!tab.Select(id))
                StatusMessage = $"{type} #{id} not found";
        }

        /// <summary>Shows an entity that may have no number (an event).</summary>
        public void NavigateToEntity(IDEntity entity)
        {
            var tab = TabOf(entity.Kind);
            if (tab == null)
                return;
            SelectedTab = tab;
            if (!tab.Select(entity))
                StatusMessage = $"{entity.Kind} not found";
        }

        private void OnSelected(EntityTypeTab tab, EntityListItem item)
        {
            if (!_travelling)
            {
                if (_current is { } c && !ReferenceEquals(c.Item, item))
                    _back.Add(c);
                _forward.Clear();
            }
            _current = (tab, item);
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(SelectedPage));
        }

        public void GoBack() => Travel(_back, _forward);

        public void GoForward() => Travel(_forward, _back);

        private void Travel(List<(EntityTypeTab Tab, EntityListItem Item)> from, List<(EntityTypeTab Tab, EntityListItem Item)> to)
        {
            if (from.Count == 0)
                return;
            var target = from[^1];
            from.RemoveAt(from.Count - 1);
            if (_current is { } c)
                to.Add(c);
            _travelling = true;
            try
            {
                SelectedTab = target.Tab;
                target.Tab.SelectedItem = target.Item;
                _current = target;
            }
            finally
            {
                _travelling = false;
            }
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>The mod's header (#modname, #description, #version, #domversion, #icon), edited with undo.</summary>
    public sealed class ModInfoViewModel : INotifyPropertyChanged
    {
        private readonly EditorSession _session;
        private readonly MainWindowViewModel _main;

        public ModInfoViewModel(EditorSession session, MainWindowViewModel main)
        {
            _session = session;
            _main = main;
            session.Changed += _ => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title => "Mod Info";

        public string? ModName { get => _session.Mod.ModName; set => Set(Command.MODNAME, value); }
        public string? ModDescription { get => _session.Mod.Description; set => Set(Command.DESCRIPTION, value); }
        public string? ModVersion { get => _session.Mod.Version; set => Set(Command.VERSION, value); }
        public string? ModDomVersion { get => _session.Mod.DomVersion; set => Set(Command.DOMVERSION, value); }
        public string? ModIcon { get => _session.Mod.Icon; set => Set(Command.ICON, value); }
        public string? CurrentFilePath => _session.FilePath;

        private void Set(Command field, string? value)
        {
            var error = _session.Edit(ed => ed.SetModInfo(field, value));
            if (error != null)
                _main.StatusMessage = error;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
