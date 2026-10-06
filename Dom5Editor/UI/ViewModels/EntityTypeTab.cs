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
            // (blesses can only be selected; a template is made for a nation)
            NewCommand = new RelayCommand(CreateNew, () => type != EntityType.BLESS && type != EntityType.TEMPLATE);
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

        private string _facet = "All";

        /// <summary>A type's own filter ("Rituals", "In a chain"), "All" for none; see <see cref="Facets"/>.</summary>
        public string Facet { get => _facet; set { _facet = value ?? "All"; OnPropertyChanged(); } }

        /// <summary>The type's own filters, or null: events by kind and chain, spells by kind.</summary>
        public IReadOnlyList<string>? Facets => Type switch
        {
            EntityType.EVENT => new[] { "All", "Good", "Bad", "Always", "Global", "In a chain", "Started by a spell", "With problems" },
            EntityType.SPELL => new[] { "All", "Combat spells", "Rituals", "Global enchantments", "Summons" },
            EntityType.MONSTER => new[] { "All", "Mages", "Priests", "Pretenders", "Recruitable" },
            EntityType.ITEM => new[] { "All", "Weapons", "Missile weapons", "Shields", "Armor", "Helmets and crowns", "Boots", "Misc items" },
            EntityType.WEAPON => new[] { "All", "Melee", "Missile" },
            EntityType.ARMOR => new[] { "All", "Shields", "Body armor", "Helmets", "Barding" },
            EntityType.SITE => new[] { "All", "Fire", "Air", "Water", "Earth", "Astral", "Death", "Nature", "Glamour", "Blood", "Holy" },
            EntityType.NATION => new[] { "All", "Early era", "Middle era", "Late era" },
            _ => null,
        };

        private static readonly Command[] RecruitCommands = { Command.ADDRECUNIT, Command.ADDRECCOM, Command.ADDFOREIGNUNIT, Command.ADDFOREIGNCOM };

        /// <summary>Whether a row is in one of the type's own filters.</summary>
        private bool InFacet(EntityListItem item, string facet)
        {
            if (Type == EntityType.EVENT)
            {
                var graph = _session.Events;
                var e = _session.Editor.OwnEntity(item.Entity) ?? item.Entity;
                var rarity = Dom5Edit.Events.EventInfo.Rarity(graph.LinesOf(e)) ?? 0;
                return facet switch
                {
                    "Good" => Dom5Edit.Events.EventInfo.IsGood(rarity),
                    "Bad" => Dom5Edit.Events.EventInfo.IsBad(rarity),
                    "Always" => rarity == 0 || rarity == 5,
                    "Global" => Dom5Edit.Events.EventInfo.IsGlobal(rarity),
                    "In a chain" => graph.ChainOf(e) != null,
                    "Started by a spell" => graph.To(e).Any(l => !l.IsEventToEvent),
                    "With problems" => graph.ProblemsOf(e).Any(),
                    _ => true,
                };
            }
            if (Type != EntityType.SPELL)
            {
                var r = _session.Resolve(item.Entity);
                long N(Command c) => Dom5Edit.Events.EventInfo.Number(r.Get(c)?.Property) ?? -1;
                IEnumerable<long> Paths() => r.GetAll(Command.MAGICSKILL).Select(v => Dom5Edit.Events.EventInfo.Number(v.Property) ?? -1);
                return (Type, facet) switch
                {
                    (EntityType.MONSTER, "Mages") => Paths().Any(p => p >= 0 && p <= 8) || r.Has(Command.CUSTOMMAGIC),
                    (EntityType.MONSTER, "Priests") => Paths().Any(p => p == 9),
                    (EntityType.MONSTER, "Pretenders") => r.Has(Command.PATHCOST) || r.Has(Command.STARTDOM),
                    (EntityType.MONSTER, "Recruitable") => item.ID > 0 && _session.Usage.UsedBy(EntityType.MONSTER, item.ID).Any(u => RecruitCommands.Contains(u.Via)),
                    (EntityType.ITEM, "Weapons") => N(Command.TYPE) is 1 or 2,
                    (EntityType.ITEM, "Missile weapons") => N(Command.TYPE) == 3,
                    (EntityType.ITEM, "Shields") => N(Command.TYPE) == 4,
                    (EntityType.ITEM, "Armor") => N(Command.TYPE) is 5 or 10,
                    (EntityType.ITEM, "Helmets and crowns") => N(Command.TYPE) is 6 or 9,
                    (EntityType.ITEM, "Boots") => N(Command.TYPE) == 7,
                    (EntityType.ITEM, "Misc items") => N(Command.TYPE) == 8,
                    (EntityType.WEAPON, "Melee") => N(Command.RANGE) <= 0,
                    (EntityType.WEAPON, "Missile") => N(Command.RANGE) > 0,
                    (EntityType.ARMOR, "Shields") => N(Command.TYPE) == 4,
                    (EntityType.ARMOR, "Body armor") => N(Command.TYPE) == 5,
                    (EntityType.ARMOR, "Helmets") => N(Command.TYPE) == 6,
                    (EntityType.ARMOR, "Barding") => N(Command.TYPE) == 9,
                    (EntityType.SITE, _) => Array.IndexOf(new[] { "Fire", "Air", "Water", "Earth", "Astral", "Death", "Nature", "Glamour", "Blood", "Holy" }, facet) is int p && p >= 0 && N(Command.PATH) == p,
                    (EntityType.NATION, "Early era") => N(Command.ERA) == 1,
                    (EntityType.NATION, "Middle era") => N(Command.ERA) == 2,
                    (EntityType.NATION, "Late era") => N(Command.ERA) == 3,
                    _ => true,
                };
            }
            if (Type == EntityType.SPELL)
            {
                var r = _session.Resolve(item.Entity);
                long effect = Dom5Edit.Events.EventInfo.Number(r.Get(Command.EFFECT)?.Property) ?? 0;
                var kind = Data.GameTables.EffectOf((int)effect);
                return facet switch
                {
                    "Combat spells" => effect < 10000,
                    "Rituals" => effect >= 10000,
                    "Global enchantments" => Dom5Edit.Events.EventInfo.IsEnchantmentEffect(effect),
                    "Summons" => kind?.ArgumentType == "unit_id",
                    _ => true,
                };
            }
            return true;
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
            bool held = HasNumber(entity) && set.TryGetValue(entity.ID, out var byId) ? ReferenceEquals(byId, entity)
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
                // (an event made next to another is sorted there: its place in the file)
                added.Order = Type == EntityType.EVENT && _session.Events.IndexOf(entity) is int at && at >= 0 ? at : _items!.Count;
                _byKey[key] = added;
                _items!.Add(added);
            }
        }

        private ObservableCollection<EntityListItem> Build()
        {
            var list = new List<EntityListItem>();
            var mod = _session.Mod;
            var own = mod.Database.TryGetValue(Type, out var set) ? set.GetFullList() : new List<IDEntity>();
            var ownById = own.Where(HasNumber).GroupBy(e => e.ID).ToDictionary(g => g.Key, g => g.First());
            if (VanillaLoader.Vanilla?.Database.TryGetValue(Type, out var vanillaSet) == true)
            {
                foreach (var v in vanillaSet.GetFullList())
                {
                    if (ownById.TryGetValue(v.ID, out var changed))
                        list.Add(new EntityListItem(Type, changed, NameOf(changed), isVanilla: true, isModified: true));
                    else
                        list.Add(new EntityListItem(Type, v, Type == EntityType.EVENT ? NameOf(v) : string.IsNullOrEmpty(v.Name) ? $"#{v.ID}" : v.Name, isVanilla: true, isModified: false));
                }
            }
            var listed = new HashSet<int>(list.Select(i => i.ID));
            // the mod's own; a #select of one the loaded vanilla data lacks (poptypes, events) is
            // still the game's: listed as vanilla, changed
            foreach (var e in own)
                if (!HasNumber(e) || !listed.Contains(e.ID))
                {
                    bool game = e.Selected && !HasVanillaData(Type);
                    list.Add(new EntityListItem(Type, e, NameOf(e), isVanilla: game, isModified: game));
                }
            int order = 0;
            foreach (var item in list)
            {
                Equip(item);
                item.Order = order++;
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
            item.FacetMatcher = InFacet;
            if (Type == EntityType.EVENT)
                item.TextMatcher = (i, text) => Dom5Edit.Events.EventInfo.Message(_session.Events.LinesOf(_session.Editor.OwnEntity(i.Entity) ?? i.Entity)) is string msg
                    && msg.Contains(text, StringComparison.OrdinalIgnoreCase);
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
            var detail = string.Join("  ", fields.Select(f => r.Get(f.Item1) is { } v ? $"{f.Item2} {v.Arguments}" : null).Where(x => x != null));
            if (Type == EntityType.MONSTER)
            {
                // a mage's paths, as the game abbreviates them: F3 S2, +2 random
                var paths = string.Join(" ", r.GetAll(Command.MAGICSKILL).Select(v => v.Property is IntIntProperty p && p.Value1 >= 0 && p.Value1 <= 9
                    ? $"{"FAWESDNGBH"[p.Value1]}{p.Value2}" : null).Where(x => x != null));
                int random = r.GetAll(Command.CUSTOMMAGIC).Count();
                if (paths.Length > 0 || random > 0)
                    detail += "   " + paths + (random > 0 ? $" +{random}" : "");
            }
            return detail;
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
            (Type == EntityType.EVENT ? id >= 0 : id > 0) && VanillaLoader.Vanilla?.Database.TryGetValue(Type, out var set) == true && set.TryGetValue(id, out var v) ? v : null;

        private object Key(IDEntity e) => HasNumber(e) ? e.ID : e;

        /// <summary>Whether the entity has a number in game: an ID, or a game event's number (from 0; a #newevent has none).</summary>
        private bool HasNumber(IDEntity e) => Type == EntityType.EVENT ? e.ID >= 0 && e.Selected : e.ID > 0;

        /// <summary>The entity's name in game (a copy's may come from its source).</summary>
        private string NameOf(IDEntity entity)
        {
            // an event has no name: its title comes from its message
            if (Type == EntityType.EVENT)
                return EventPageViewModel.TitleOf(entity, EventPageViewModel.LinesOf(_session.Resolve(entity)));
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
