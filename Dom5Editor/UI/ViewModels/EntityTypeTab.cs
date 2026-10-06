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

        // the list's filter, kept per tab (the list view is rebuilt when the tab is shown again)
        private string _searchText = "";
        private bool _showVanilla = true, _showModified = true, _showNew = true;
        public string SearchText { get => _searchText; set { _searchText = value ?? ""; OnPropertyChanged(); } }
        public bool ShowVanilla { get => _showVanilla; set { _showVanilla = value; OnPropertyChanged(); } }
        public bool ShowModified { get => _showModified; set { _showModified = value; OnPropertyChanged(); } }
        public bool ShowNew { get => _showNew; set { _showNew = value; OnPropertyChanged(); } }
        private string _sortBy = "ID";
        public string SortBy { get => _sortBy; set { _sortBy = value ?? "ID"; OnPropertyChanged(); } }

        public System.Windows.Input.ICommand NewCommand { get; }
        public System.Windows.Input.ICommand DeleteCommand { get; }

        /// <summary>Why the last new/delete couldn't be done, for the status bar.</summary>
        public string? LastError { get; private set; }

        private void CreateNew()
        {
            IDEntity? made = null;
            LastError = _session.Edit(ed =>
            {
                var edit = ed.Create(Type, $"New {Singular.ToLowerInvariant()}", out var m);
                made = m;
                return edit;
            });
            if (made != null)
                SelectedItem = Items.FirstOrDefault(i => ReferenceEquals(i.Entity, made));
        }

        /// <summary>Asks the user to confirm something (set by the window; with none, the answer is yes).</summary>
        public static Func<string, bool>? Confirm { get; set; }

        /// <summary>Raised with a message for the status bar.</summary>
        public event Action<string>? Status;

        private void Delete(EntityListItem? item)
        {
            if (item == null)
                return;
            var users = item.ID > 0 ? _session.Usage.UsedBy(Type, item.ID).Select(u => (u.Type, u.Id)).Distinct().Count() : 0;
            string what = item.IsNew ? $"Delete {item.DisplayName} #{item.ID}" : $"Drop this mod's changes to {item.DisplayName} #{item.ID}";
            if (users > 0 && item.IsNew && Confirm?.Invoke($"{what}?\n\n{users} entities refer to it; their references would point at nothing. (Undo brings it back.)") == false)
                return;
            LastError = _session.Edit(ed => ed.Delete(item.Entity));
            Status?.Invoke(LastError ?? (item.IsNew ? $"Deleted {item.DisplayName} #{item.ID}" + (users > 0 ? $"; {users} entities still refer to it" : "")
                                                     : $"Dropped the mod's changes to {item.DisplayName} #{item.ID}"));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Raised when the user (or navigation) selects an entity, for the back/forward history.</summary>
        public event Action<EntityTypeTab, EntityListItem>? Selected;

        public EntityType Type { get; }
        public string Title { get; }

        /// <summary>One of the type ("Monster" for "Monsters", "Mercenary" for "Mercenaries").</summary>
        public string Singular => Title.EndsWith("ies") ? Title[..^3] + "y" : Title.EndsWith("sses") || Title.EndsWith("ses") ? Title[..^2] : Title.TrimEnd('s');

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
        /// <summary>Selects an entity's row by the entity itself (or its ID).</summary>
        public bool Select(IDEntity entity)
        {
            var item = Items.FirstOrDefault(i => ReferenceEquals(i.Entity, entity)) ?? (entity.ID > 0 ? Items.FirstOrDefault(i => i.ID == entity.ID) : null);
            if (item == null)
                return false;
            SelectedItem = item;
            return true;
        }

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
            var set = _session.Mod.Database[Type];
            bool held = entity.ID > 0 && set.TryGetValue(entity.ID, out var byId) ? ReferenceEquals(byId, entity)
                : set.GetFullList().Any(e => ReferenceEquals(e, entity));
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
                item.Refresh();
                return;
            }
            if (held)
            {
                bool game = vanilla != null || entity.Selected && !HasVanillaData(Type);
                var added = new EntityListItem(Type, entity, NameOf(entity), isVanilla: game, isModified: game);
                Equip(added);
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
            {
                Equip(item);
                _byKey[Key(item.Entity)] = item;
            }
            return new ObservableCollection<EntityListItem>(list);
        }

        /// <summary>
        /// Whether the vanilla data has this type (vanilla.dm has monsters, weapons, armor, spells,
        /// items, sites and nations). A mod's #select of an ID it doesn't have makes a new entity; for
        /// a type it lacks (poptypes, events, ...) the #select changes a game entity we can't show.
        /// </summary>
        public static bool HasVanillaData(EntityType type) =>
            VanillaLoader.Vanilla?.Database.TryGetValue(type, out var set) == true && set.GetFullList().Count > 0;

        /// <summary>Gives a row its detail line (key stats) and sprite, worked out when the row is shown.</summary>
        private void Equip(EntityListItem item)
        {
            item.DetailProvider = Detail;
            if (Type == EntityType.MONSTER || Type == EntityType.ITEM)
                item.SpriteProvider = i =>
                {
                    var r = _session.Resolve(i.Entity);
                    var c = Type == EntityType.MONSTER ? Command.SPR1 : Command.SPR;
                    var p = r.Get(c)?.Property ?? r.Assets.GetValueOrDefault(c);
                    return p is FilePathProperty f ? Sprites.SpriteLoader.Load(f.Value, _session.Mod.FullFilePath) : null;
                };
        }

        /// <summary>A row's key stats, by type.</summary>
        private string Detail(EntityListItem item)
        {
            (Command, string)[] fields = Type switch
            {
                EntityType.MONSTER => new[] { (Command.HP, "hp"), (Command.ATT, "att"), (Command.DEF, "def"), (Command.PROT, "prot"), (Command.SIZE, "size") },
                EntityType.WEAPON => new[] { (Command.DMG, "dmg"), (Command.ATT, "att"), (Command.DEF, "def"), (Command.LEN, "len") },
                EntityType.ARMOR => new[] { (Command.PROT, "prot"), (Command.DEF, "def"), (Command.ENC, "enc") },
                EntityType.SPELL => new[] { (Command.RESEARCHLEVEL, "research") },
                EntityType.ITEM => new[] { (Command.CONSTLEVEL, "const") },
                EntityType.SITE => new[] { (Command.LEVEL, "level") },
                _ => Array.Empty<(Command, string)>(),
            };
            if (Type == EntityType.EVENT)
                return EventDetail(item);
            if (fields.Length == 0)
                return "";
            var r = _session.Resolve(item.Entity);
            return string.Join("  ", fields.Select(f => r.Get(f.Item1) is { } v ? $"{f.Item2} {v.Arguments}" : null).Where(x => x != null));
        }

        /// <summary>An event's row: how it's rolled, and what starts it (an enchantment, a code, a spell).</summary>
        private string EventDetail(EntityListItem item)
        {
            var lines = _session.Resolve(item.Entity).Values.Select(v => v.Property).ToList();
            var parts = new List<string> { Dom5Edit.Events.EventInfo.RarityName(Dom5Edit.Events.EventInfo.Rarity(lines)) };
            foreach (var p in lines)
            {
                var n = Dom5Edit.Events.EventInfo.Number(p);
                if (Dom5Edit.Events.EventInfo.EnchantmentCheckers.Contains(p.Command) && p.Command != Command.REQ_NOENCH)
                    parts.Add($"ench {n}");
                else if (Dom5Edit.Events.EventInfo.CodeCheckers.Contains(p.Command) && n != 0)
                    parts.Add($"needs code {n}");
                else if (Dom5Edit.Events.EventInfo.CodeSetters.Contains(p.Command) && n != 0)
                    parts.Add($"sets code {n}");
                else if (Dom5Edit.Events.EventInfo.Delays.Contains(p.Command))
                    parts.Add($"delay {n}");
                else if (p.Command == Command.ID)
                    parts.Add($"spell event {n}");
            }
            return string.Join(" · ", parts.Distinct().Take(4));
        }

        private IDEntity? VanillaOf(int id) =>
            id > 0 && VanillaLoader.Vanilla?.Database.TryGetValue(Type, out var set) == true && set.TryGetValue(id, out var v) ? v : null;

        private static object Key(IDEntity e) => e.ID > 0 ? e.ID : e;

        /// <summary>The entity's name in game (a copy's may come from its source).</summary>
        private string NameOf(IDEntity entity)
        {
            // an event has no name: its title comes from its message
            if (Type == EntityType.EVENT)
                return Dom5Edit.Events.EventInfo.Title(_session.Resolve(entity).Values.Select(v => v.Property).ToList());
            var name = _session.Resolve(entity).Get(Command.NAME)?.Property is StringProperty s ? s.Value : null;
            if (string.IsNullOrEmpty(name))
                name = entity.HeaderName;
            if (string.IsNullOrEmpty(name))
                return entity.ID > 0 ? $"{Singular} {entity.ID}" : $"(unnamed {Singular.ToLowerInvariant()})";
            return name!;
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
