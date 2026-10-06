using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Editing;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Session;

namespace Dom5Editor.UI.ViewModels
{
    /// <summary>
    /// One entity type's tab: the list of entities (vanilla and the mod's) and the page of the
    /// selected one. The list is built when the tab is first shown and kept up to date as edits
    /// create, delete, rename or first change entities.
    /// </summary>
    public sealed class EntityTypeTab : INotifyPropertyChanged
    {
        private readonly EditorSession _session;
        private ObservableCollection<EntityListItem>? _items;
        private readonly Dictionary<object, EntityListItem> _byKey = new Dictionary<object, EntityListItem>();
        private EntityListItem? _selected;
        private EntityPageViewModel? _page;

        public EntityTypeTab(EditorSession session, EntityType type, string title)
        {
            _session = session;
            Type = type;
            Title = title;
            NewCommand = new RelayCommand(CreateNew);
            DeleteCommand = new RelayCommand<object>(o => Delete(o as EntityListItem), o => o is EntityListItem i && (i.IsNew || i.IsModified));
        }

        public System.Windows.Input.ICommand NewCommand { get; }
        public System.Windows.Input.ICommand DeleteCommand { get; }

        /// <summary>Why the last new/delete couldn't be done, for the status bar.</summary>
        public string? LastError { get; private set; }

        private void CreateNew()
        {
            IDEntity? made = null;
            LastError = _session.Edit(ed =>
            {
                var edit = ed.Create(Type, $"New {Title.TrimEnd('s').ToLowerInvariant()}", out var m);
                made = m;
                return edit;
            });
            if (made != null)
                SelectedItem = Items.FirstOrDefault(i => ReferenceEquals(i.Entity, made));
        }

        private void Delete(EntityListItem? item)
        {
            if (item == null)
                return;
            LastError = _session.Edit(ed => ed.Delete(item.Entity));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raised when the user (or navigation) selects an entity, for the back/forward history.</summary>
        public event Action<EntityTypeTab, EntityListItem>? Selected;

        public EntityType Type { get; }
        public string Title { get; }

        public ObservableCollection<EntityListItem> Items => _items ??= Build();

        public EntityListItem? SelectedItem
        {
            get => _selected;
            set
            {
                if (ReferenceEquals(_selected, value))
                    return;
                _selected = value;
                Page = value == null ? null : EntityPages.Create(_session, value);
                OnPropertyChanged();
                if (value != null)
                    Selected?.Invoke(this, value);
            }
        }

        /// <summary>The selected entity's page.</summary>
        public EntityPageViewModel? Page
        {
            get => _page;
            private set
            {
                _page?.Detach();
                _page = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Selects the entity with this ID (false if the list has none).</summary>
        public bool Select(int id)
        {
            var item = Items.FirstOrDefault(i => i.ID == id);
            if (item == null)
                return false;
            SelectedItem = item;
            return true;
        }

        /// <summary>Keeps the list in step with an edit (and an undo or redo of one).</summary>
        public void OnChanged(IModEdit edit)
        {
            if (_items == null)
                return;
            foreach (var entity in edit.Entities.Where(e => e.Kind == Type))
                Update(entity);
        }

        private void Update(IDEntity entity)
        {
            var key = Key(entity);
            bool held = _session.Mod.Database[Type].GetFullList().Any(e => ReferenceEquals(e, entity));
            var vanilla = VanillaOf(entity.ID);
            if (_byKey.TryGetValue(key, out var item))
            {
                if (!held && vanilla == null)
                {
                    // a mod entity deleted (or an undone create)
                    _items!.Remove(item);
                    _byKey.Remove(key);
                    if (ReferenceEquals(_selected, item))
                        SelectedItem = null;
                    return;
                }
                item.Entity = held ? entity : vanilla!;
                item.IsModified = item.IsVanilla && held;
                item.DisplayName = NameOf(item.Entity);
                return;
            }
            if (held)
            {
                bool game = vanilla != null || entity.Selected && !HasVanillaData(Type);
                var added = new EntityListItem(Type, entity, NameOf(entity), isVanilla: game, isModified: game);
                _byKey[key] = added;
                _items!.Add(added);
            }
        }

        private ObservableCollection<EntityListItem> Build()
        {
            var list = new List<EntityListItem>();
            var mod = _session.Mod;
            var own = mod.Database.TryGetValue(Type, out var set) ? set.GetFullList() : new List<IDEntity>();
            var ownById = own.Where(e => e.ID > 0).GroupBy(e => e.ID).ToDictionary(g => g.Key, g => g.First());
            if (VanillaLoader.Vanilla?.Database.TryGetValue(Type, out var vanillaSet) == true)
            {
                foreach (var v in vanillaSet.GetFullList())
                {
                    if (ownById.TryGetValue(v.ID, out var changed))
                        list.Add(new EntityListItem(Type, changed, NameOf(changed), isVanilla: true, isModified: true));
                    else
                        list.Add(new EntityListItem(Type, v, string.IsNullOrEmpty(v.Name) ? $"#{v.ID}" : v.Name, isVanilla: true, isModified: false));
                }
            }
            var listed = new HashSet<int>(list.Select(i => i.ID));
            // the mod's own; a #select of one the loaded vanilla data lacks (poptypes, events) is
            // still the game's: listed as vanilla, changed
            foreach (var e in own)
                if (e.ID <= 0 || !listed.Contains(e.ID))
                {
                    bool game = e.Selected && !HasVanillaData(Type);
                    list.Add(new EntityListItem(Type, e, NameOf(e), isVanilla: game, isModified: game));
                }
            foreach (var item in list)
                _byKey[Key(item.Entity)] = item;
            return new ObservableCollection<EntityListItem>(list);
        }

        /// <summary>
        /// Whether the vanilla data has this type (vanilla.dm has monsters, weapons, armor, spells,
        /// items, sites and nations). A mod's #select of an ID it doesn't have makes a new entity; for
        /// a type it lacks (poptypes, events, ...) the #select changes a game entity we can't show.
        /// </summary>
        public static bool HasVanillaData(EntityType type) =>
            VanillaLoader.Vanilla?.Database.TryGetValue(type, out var set) == true && set.GetFullList().Count > 0;

        private IDEntity? VanillaOf(int id) =>
            id > 0 && VanillaLoader.Vanilla?.Database.TryGetValue(Type, out var set) == true && set.TryGetValue(id, out var v) ? v : null;

        private static object Key(IDEntity e) => e.ID > 0 ? e.ID : e;

        /// <summary>The entity's name in game (a copy's may come from its source).</summary>
        private string NameOf(IDEntity entity)
        {
            var name = _session.Resolve(entity).Get(Command.NAME)?.Property is StringProperty s ? s.Value : null;
            return string.IsNullOrEmpty(name) ? $"{Title.TrimEnd('s')} {entity.ID}" : name!;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
