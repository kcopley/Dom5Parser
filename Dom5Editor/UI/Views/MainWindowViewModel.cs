using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Validation;
using Dom5Editor.EditCommands;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Manages mod loading, entity collections, and undo/redo state.
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Mod _mod;
        private string _currentFilePath;
        private string _statusMessage = "Ready";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            History = new CommandHistory();
            Changes = new ChangesMod();
            History.ChangesMod = Changes;

            History.HistoryChanged += () =>
            {
                OnPropertyChanged(nameof(CanUndo));
                OnPropertyChanged(nameof(CanRedo));
                OnPropertyChanged(nameof(IsDirty));
                OnPropertyChanged(nameof(UndoDescription));
                OnPropertyChanged(nameof(RedoDescription));
            };
        }

        // ========================================
        // Mod Management
        // ========================================

        public CommandHistory History { get; }
        public ChangesMod Changes { get; private set; }

        public bool HasMod => _mod != null;
        public string CurrentFilePath => _currentFilePath;

        public string ModName
        {
            get => _mod?.ModName;
            set
            {
                if (_mod != null)
                {
                    _mod.ModName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ModDescription
        {
            get => _mod?.Description;
            set
            {
                if (_mod != null)
                {
                    _mod.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ModVersion
        {
            get => _mod?.Version;
            set
            {
                if (_mod != null)
                {
                    _mod.Version = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ModDomVersion
        {
            get => _mod?.DomVersion;
            set
            {
                if (_mod != null)
                {
                    _mod.DomVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ModIcon
        {
            get => _mod?.Icon;
            set
            {
                if (_mod != null)
                {
                    _mod.Icon = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public int EntityCount
        {
            get
            {
                if (_mod == null) return 0;
                int count = 0;
                foreach (var kvp in _mod.Database)
                {
                    count += kvp.Value.GetFullList().Count;
                }
                return count;
            }
        }

        // ========================================
        // Undo/Redo
        // ========================================

        public bool CanUndo => History.CanUndo;
        public bool CanRedo => History.CanRedo;
        public bool IsDirty => History.IsDirty;
        public string UndoDescription => History.UndoDescription;
        public string RedoDescription => History.RedoDescription;

        public void Undo()
        {
            if (CanUndo)
            {
                History.Undo();
                StatusMessage = $"Undone: {History.RedoDescription}";
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                History.Redo();
                StatusMessage = $"Redone: {History.UndoDescription}";
            }
        }

        // ========================================
        // Entity Collections
        // ========================================

        private ObservableCollection<MonsterViewModel> _monsters;
        public ObservableCollection<MonsterViewModel> Monsters
        {
            get => _monsters;
            private set { _monsters = value; OnPropertyChanged(); }
        }

        private MonsterViewModel _selectedMonster;
        public MonsterViewModel SelectedMonster
        {
            get => _selectedMonster;
            set { _selectedMonster = value; OnPropertyChanged(); }
        }

        private ObservableCollection<WeaponViewModel> _weapons;
        public ObservableCollection<WeaponViewModel> Weapons
        {
            get => _weapons;
            private set { _weapons = value; OnPropertyChanged(); }
        }

        private WeaponViewModel _selectedWeapon;
        public WeaponViewModel SelectedWeapon
        {
            get => _selectedWeapon;
            set { _selectedWeapon = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ArmorViewModel> _armors;
        public ObservableCollection<ArmorViewModel> Armors
        {
            get => _armors;
            private set { _armors = value; OnPropertyChanged(); }
        }

        private ArmorViewModel _selectedArmor;
        public ArmorViewModel SelectedArmor
        {
            get => _selectedArmor;
            set { _selectedArmor = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SpellViewModel> _spells;
        public ObservableCollection<SpellViewModel> Spells
        {
            get => _spells;
            private set { _spells = value; OnPropertyChanged(); }
        }

        private SpellViewModel _selectedSpell;
        public SpellViewModel SelectedSpell
        {
            get => _selectedSpell;
            set { _selectedSpell = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ItemViewModel> _items;
        public ObservableCollection<ItemViewModel> Items
        {
            get => _items;
            private set { _items = value; OnPropertyChanged(); }
        }

        private ItemViewModel _selectedItem;
        public ItemViewModel SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SiteViewModel> _sites;
        public ObservableCollection<SiteViewModel> Sites
        {
            get => _sites;
            private set { _sites = value; OnPropertyChanged(); }
        }

        private SiteViewModel _selectedSite;
        public SiteViewModel SelectedSite
        {
            get => _selectedSite;
            set { _selectedSite = value; OnPropertyChanged(); }
        }

        private ObservableCollection<NationViewModel> _nations;
        public ObservableCollection<NationViewModel> Nations
        {
            get => _nations;
            private set { _nations = value; OnPropertyChanged(); }
        }

        private NationViewModel _selectedNation;
        public NationViewModel SelectedNation
        {
            get => _selectedNation;
            set { _selectedNation = value; OnPropertyChanged(); }
        }

        private ObservableCollection<EventViewModel> _events;
        public ObservableCollection<EventViewModel> Events
        {
            get => _events;
            private set { _events = value; OnPropertyChanged(); }
        }

        private EventViewModel _selectedEvent;
        public EventViewModel SelectedEvent
        {
            get => _selectedEvent;
            set { _selectedEvent = value; OnPropertyChanged(); }
        }

        private ObservableCollection<MercenaryViewModel> _mercenaries;
        public ObservableCollection<MercenaryViewModel> Mercenaries
        {
            get => _mercenaries;
            private set { _mercenaries = value; OnPropertyChanged(); }
        }

        private MercenaryViewModel _selectedMercenary;
        public MercenaryViewModel SelectedMercenary
        {
            get => _selectedMercenary;
            set { _selectedMercenary = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PoptypeViewModel> _poptypes;
        public ObservableCollection<PoptypeViewModel> Poptypes
        {
            get => _poptypes;
            private set { _poptypes = value; OnPropertyChanged(); }
        }

        private PoptypeViewModel _selectedPoptype;
        public PoptypeViewModel SelectedPoptype
        {
            get => _selectedPoptype;
            set { _selectedPoptype = value; OnPropertyChanged(); }
        }

        private ObservableCollection<NametypeViewModel> _nametypes;
        public ObservableCollection<NametypeViewModel> Nametypes
        {
            get => _nametypes;
            private set { _nametypes = value; OnPropertyChanged(); }
        }

        private NametypeViewModel _selectedNametype;
        public NametypeViewModel SelectedNametype
        {
            get => _selectedNametype;
            set { _selectedNametype = value; OnPropertyChanged(); }
        }

        private ObservableCollection<BlessViewModel> _blesses;
        public ObservableCollection<BlessViewModel> Blesses
        {
            get => _blesses;
            private set { _blesses = value; OnPropertyChanged(); }
        }

        private BlessViewModel _selectedBless;
        public BlessViewModel SelectedBless
        {
            get => _selectedBless;
            set { _selectedBless = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TemplateViewModel> _templates;
        public ObservableCollection<TemplateViewModel> Templates
        {
            get => _templates;
            private set { _templates = value; OnPropertyChanged(); }
        }

        private TemplateViewModel _selectedTemplate;
        public TemplateViewModel SelectedTemplate
        {
            get => _selectedTemplate;
            set { _selectedTemplate = value; OnPropertyChanged(); }
        }

        // ========================================
        // Entity Reference Caches (for dropdown performance)
        // ========================================

        /// <summary>
        /// Cached list of all available weapons (vanilla + mod) for dropdown binding.
        /// Built once when mod loads, shared across all ViewModels.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedWeapons { get; private set; }

        /// <summary>
        /// Cached list of all available armor (vanilla + mod) for dropdown binding.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedArmors { get; private set; }

        /// <summary>
        /// Cached list of all available monsters (vanilla + mod) for dropdown binding.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedMonsters { get; private set; }

        /// <summary>
        /// Cached list of all available items (vanilla + mod) for dropdown binding.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedItems { get; private set; }

        /// <summary>
        /// Cached list of all available spells (vanilla + mod) for dropdown binding.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedSpells { get; private set; }

        /// <summary>
        /// Cached list of all available sites (vanilla + mod) for dropdown binding.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedSites { get; private set; }

        /// <summary>
        /// Cached list of all available nations (vanilla + mod) for dropdown binding.
        /// </summary>
        public IReadOnlyList<AvailableEquipmentItem> CachedNations { get; private set; }

        // ========================================
        // Entity Navigation
        // ========================================

        private EntityType _selectedEntityType = EntityType.MONSTER;
        public EntityType SelectedEntityType
        {
            get => _selectedEntityType;
            set
            {
                if (_selectedEntityType != value)
                {
                    _selectedEntityType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedEntityTypeIndex));
                }
            }
        }

        /// <summary>
        /// Tab index for XAML binding. Maps to/from EntityType.
        /// </summary>
        public int SelectedEntityTypeIndex
        {
            get => EntityTypeToTabIndex(_selectedEntityType);
            set
            {
                var newType = TabIndexToEntityType(value);
                if (newType.HasValue && newType.Value != _selectedEntityType)
                {
                    _selectedEntityType = newType.Value;
                    OnPropertyChanged(nameof(SelectedEntityType));
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Navigate to a specific entity by type and ID.
        /// </summary>
        public void NavigateToEntity(EntityType entityType, int id)
        {
            // Check if entity type has a tab (skip dependent entities)
            if (!HasTabForEntityType(entityType))
                return; // Silent no-op for Montag, Enchantment, etc.

            SelectedEntityType = entityType;
            bool found = SelectEntityById(entityType, id);

            if (!found)
            {
                StatusMessage = $"Entity #{id} not found";
            }
        }

        /// <summary>
        /// Select entity in the appropriate collection by ID.
        /// Returns true if entity was found and selected.
        /// </summary>
        private bool SelectEntityById(EntityType entityType, int id)
        {
            switch (entityType)
            {
                case EntityType.MONSTER:
                    var monster = Monsters?.FirstOrDefault(m => m.ID == id);
                    if (monster != null) { SelectedMonster = monster; return true; }
                    return false;
                case EntityType.WEAPON:
                    var weapon = Weapons?.FirstOrDefault(w => w.ID == id);
                    if (weapon != null) { SelectedWeapon = weapon; return true; }
                    return false;
                case EntityType.ARMOR:
                    var armor = Armors?.FirstOrDefault(a => a.ID == id);
                    if (armor != null) { SelectedArmor = armor; return true; }
                    return false;
                case EntityType.SPELL:
                    var spell = Spells?.FirstOrDefault(s => s.ID == id);
                    if (spell != null) { SelectedSpell = spell; return true; }
                    return false;
                case EntityType.ITEM:
                    var item = Items?.FirstOrDefault(i => i.ID == id);
                    if (item != null) { SelectedItem = item; return true; }
                    return false;
                case EntityType.SITE:
                    var site = Sites?.FirstOrDefault(s => s.ID == id);
                    if (site != null) { SelectedSite = site; return true; }
                    return false;
                case EntityType.NATION:
                    var nation = Nations?.FirstOrDefault(n => n.ID == id);
                    if (nation != null) { SelectedNation = nation; return true; }
                    return false;
                case EntityType.EVENT:
                    var evt = Events?.FirstOrDefault(e => e.ID == id);
                    if (evt != null) { SelectedEvent = evt; return true; }
                    return false;
                case EntityType.MERCENARY:
                    var merc = Mercenaries?.FirstOrDefault(m => m.ID == id);
                    if (merc != null) { SelectedMercenary = merc; return true; }
                    return false;
                case EntityType.POPTYPE:
                    var poptype = Poptypes?.FirstOrDefault(p => p.ID == id);
                    if (poptype != null) { SelectedPoptype = poptype; return true; }
                    return false;
                case EntityType.NAMETYPE:
                    var nametype = Nametypes?.FirstOrDefault(n => n.ID == id);
                    if (nametype != null) { SelectedNametype = nametype; return true; }
                    return false;
                case EntityType.BLESS:
                    var bless = Blesses?.FirstOrDefault(b => b.ID == id);
                    if (bless != null) { SelectedBless = bless; return true; }
                    return false;
                case EntityType.TEMPLATE:
                    var template = Templates?.FirstOrDefault(t => t.ID == id);
                    if (template != null) { SelectedTemplate = template; return true; }
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if entity type has a corresponding tab in the UI.
        /// </summary>
        private static bool HasTabForEntityType(EntityType type)
        {
            return type switch
            {
                EntityType.MONSTER => true,
                EntityType.WEAPON => true,
                EntityType.ARMOR => true,
                EntityType.SPELL => true,
                EntityType.ITEM => true,
                EntityType.SITE => true,
                EntityType.NATION => true,
                EntityType.EVENT => true,
                EntityType.MERCENARY => true,
                EntityType.POPTYPE => true,
                EntityType.NAMETYPE => true,
                EntityType.BLESS => true,
                EntityType.TEMPLATE => true,
                // Dependent entities without tabs
                EntityType.MONTAG => false,
                EntityType.ENCHANTMENT => false,
                EntityType.RESTRICTED_ITEM => false,
                EntityType.EVENT_CODE => false,
                _ => false
            };
        }

        /// <summary>
        /// Map EntityType to tab index (matches TabControl order in MainWindow.xaml).
        /// </summary>
        private static int EntityTypeToTabIndex(EntityType type)
        {
            // Tab 0 is Mod Info (not an entity type)
            return type switch
            {
                EntityType.MONSTER => 1,
                EntityType.WEAPON => 2,
                EntityType.ARMOR => 3,
                EntityType.SPELL => 4,
                EntityType.ITEM => 5,
                EntityType.SITE => 6,
                EntityType.NATION => 7,
                EntityType.EVENT => 8,
                EntityType.MERCENARY => 9,
                EntityType.POPTYPE => 10,
                EntityType.NAMETYPE => 11,
                EntityType.BLESS => 12,
                EntityType.TEMPLATE => 13,
                _ => 1
            };
        }

        /// <summary>
        /// Map tab index to EntityType.
        /// </summary>
        private static EntityType? TabIndexToEntityType(int index)
        {
            // Tab 0 is Mod Info (not an entity type)
            return index switch
            {
                0 => null, // Mod Info tab
                1 => EntityType.MONSTER,
                2 => EntityType.WEAPON,
                3 => EntityType.ARMOR,
                4 => EntityType.SPELL,
                5 => EntityType.ITEM,
                6 => EntityType.SITE,
                7 => EntityType.NATION,
                8 => EntityType.EVENT,
                9 => EntityType.MERCENARY,
                10 => EntityType.POPTYPE,
                11 => EntityType.NAMETYPE,
                12 => EntityType.BLESS,
                13 => EntityType.TEMPLATE,
                _ => null
            };
        }

        // ========================================
        // Mod Operations
        // ========================================

        public void CreateNewMod()
        {
            _mod = new Mod();
            _currentFilePath = null;
            ClearHistory();  // Must come BEFORE InitializeCollections so VMs get the new ChangesMod
            InitializeCollections();
            StatusMessage = "Created new mod";
        }

        public void LoadMod(string filePath)
        {
            _mod = Mod.Import(filePath);
            _currentFilePath = filePath;
            ClearHistory();  // Must come BEFORE InitializeCollections so VMs get the new ChangesMod
            Changes.LoadedMod = _mod;
            InitializeCollections();
            StatusMessage = $"Loaded: {System.IO.Path.GetFileName(filePath)}";
        }

        public void SaveMod(string filePath)
        {
            using (var writer = new System.IO.StreamWriter(filePath))
            {
                var exporter = new ChangesModExporter(Changes);
                exporter.Export(writer, ModName ?? "Mod", _mod?.Description);
            }
            _currentFilePath = filePath;
            History.MarkSaved();
            StatusMessage = $"Saved: {System.IO.Path.GetFileName(filePath)}";
            OnPropertyChanged(nameof(IsDirty));
        }

        public ValidationResult Validate()
        {
            if (_mod == null) return null;

            var validator = new ModValidator();
            var results = validator.ValidateWithSummary(_mod);

            StatusMessage = $"Validation: {results.ErrorCount} errors, {results.WarningCount} warnings";

            return results;
        }

        // ========================================
        // Private Helpers
        // ========================================

        private void InitializeCollections()
        {
            // Clear selections
            SelectedMonster = null;
            SelectedWeapon = null;
            SelectedArmor = null;
            SelectedSpell = null;
            SelectedItem = null;
            SelectedSite = null;
            SelectedNation = null;
            SelectedEvent = null;
            SelectedMercenary = null;
            SelectedPoptype = null;
            SelectedNametype = null;
            SelectedBless = null;
            SelectedTemplate = null;

            // Build entity caches FIRST, before creating ViewModels
            // This ensures dropdowns are pre-populated when VMs are constructed
            BuildEntityCaches();

            // Initialize entity collections from mod + vanilla
            Monsters = LoadEntities<Monster, MonsterViewModel>(EntityType.MONSTER,
                (e, h, s) => new MonsterViewModel(e, h, s));
            Weapons = LoadEntities<Weapon, WeaponViewModel>(EntityType.WEAPON,
                (e, h, s) => new WeaponViewModel(e, h, s));
            Armors = LoadEntities<Armor, ArmorViewModel>(EntityType.ARMOR,
                (e, h, s) => new ArmorViewModel(e, h, s));
            Spells = LoadEntities<Spell, SpellViewModel>(EntityType.SPELL,
                (e, h, s) => new SpellViewModel(e, h, s));
            Items = LoadEntities<Item, ItemViewModel>(EntityType.ITEM,
                (e, h, s) => new ItemViewModel(e, h, s));
            Sites = LoadEntities<Site, SiteViewModel>(EntityType.SITE,
                (e, h, s) => new SiteViewModel(e, h, s));
            Nations = LoadEntities<Nation, NationViewModel>(EntityType.NATION,
                (e, h, s) => new NationViewModel(e, h, s));
            Events = LoadEntities<Event, EventViewModel>(EntityType.EVENT,
                (e, h, s) => new EventViewModel(e, h, s));
            Mercenaries = LoadEntities<Mercenary, MercenaryViewModel>(EntityType.MERCENARY,
                (e, h, s) => new MercenaryViewModel(e, h, s));
            Poptypes = LoadEntities<Poptype, PoptypeViewModel>(EntityType.POPTYPE,
                (e, h, s) => new PoptypeViewModel(e, h, s));
            Nametypes = LoadEntities<Nametype, NametypeViewModel>(EntityType.NAMETYPE,
                (e, h, s) => new NametypeViewModel(e, h, s));
            Blesses = LoadEntities<Bless, BlessViewModel>(EntityType.BLESS,
                (e, h, s) => new BlessViewModel(e, h, s));
            Templates = LoadEntities<Template, TemplateViewModel>(EntityType.TEMPLATE,
                (e, h, s) => new TemplateViewModel(e, h, s));

            OnPropertyChanged(nameof(HasMod));
            OnPropertyChanged(nameof(EntityCount));
        }

        private ObservableCollection<TViewModel> LoadEntities<TEntity, TViewModel>(
            EntityType type,
            System.Func<TEntity, CommandHistory, EntitySource, TViewModel> factory)
            where TEntity : IDEntity
            where TViewModel : EntityViewModel
        {
            var collection = new ObservableCollection<TViewModel>();
            var loadedIds = new HashSet<int>();

            // Get mod entity set to check for overrides
            EntitySet<IDEntity> modSet = null;
            _mod?.Database.TryGetValue(type, out modSet);

            // Load vanilla entities first
            if (VanillaLoader.Vanilla?.Database.TryGetValue(type, out var vanillaSet) == true)
            {
                foreach (var entity in vanillaSet.GetFullList())
                {
                    if (entity is TEntity typedEntity)
                    {
                        // Check if this vanilla entity has a mod override
                        IDEntity modEntity = null;
                        bool hasModOverride = modSet?.TryGetValue(typedEntity.ID, out modEntity) == true;

                        TViewModel vm;
                        if (hasModOverride && modEntity is TEntity modTypedEntity)
                        {
                            // Use the mod's version of the entity
                            vm = factory(modTypedEntity, History, EntitySource.VanillaModified);
                        }
                        else
                        {
                            // Use vanilla entity as-is
                            vm = factory(typedEntity, History, EntitySource.Vanilla);
                        }

                        // Set ChangesMod for session edit tracking
                        vm.SetChangesMod(Changes);
                        collection.Add(vm);
                        loadedIds.Add(typedEntity.ID);
                    }
                }
            }

            // Load mod entities that don't exist in vanilla (new entities from mod)
            if (modSet != null)
            {
                foreach (var entity in modSet.GetFullList())
                {
                    if (entity is TEntity typedEntity)
                    {
                        // Skip if this ID already exists from vanilla (it's just an override)
                        if (loadedIds.Contains(typedEntity.ID))
                            continue;

                        var vm = factory(typedEntity, History, EntitySource.FromMod);
                        vm.SetChangesMod(Changes);
                        collection.Add(vm);
                        loadedIds.Add(typedEntity.ID);
                    }
                }
            }

            return collection;
        }

        private void ClearHistory()
        {
            History.Clear();
            Changes = new ChangesMod { LoadedMod = _mod };
            History.ChangesMod = Changes;
        }

        /// <summary>
        /// Builds all entity reference caches for dropdown performance.
        /// Uses O(1) HashSet-based deduplication instead of O(n) List.Any().
        /// </summary>
        private void BuildEntityCaches()
        {
            CachedWeapons = BuildEntityCache<Weapon>(EntityType.WEAPON);
            CachedArmors = BuildEntityCache<Armor>(EntityType.ARMOR);
            CachedMonsters = BuildEntityCache<Monster>(EntityType.MONSTER);
            CachedItems = BuildEntityCache<Item>(EntityType.ITEM);
            CachedSpells = BuildEntityCache<Spell>(EntityType.SPELL);
            CachedSites = BuildEntityCache<Site>(EntityType.SITE);
            CachedNations = BuildEntityCache<Nation>(EntityType.NATION);

            // Notify UI that caches have been rebuilt
            OnPropertyChanged(nameof(CachedWeapons));
            OnPropertyChanged(nameof(CachedArmors));
            OnPropertyChanged(nameof(CachedMonsters));
            OnPropertyChanged(nameof(CachedItems));
            OnPropertyChanged(nameof(CachedSpells));
            OnPropertyChanged(nameof(CachedSites));
            OnPropertyChanged(nameof(CachedNations));
        }

        /// <summary>
        /// Builds a cached list of available entities for dropdown binding.
        /// </summary>
        /// <typeparam name="TEntity">The entity type (Weapon, Armor, Monster, etc.)</typeparam>
        /// <param name="type">The entity type enum value</param>
        /// <returns>Sorted read-only list of available equipment items</returns>
        private IReadOnlyList<AvailableEquipmentItem> BuildEntityCache<TEntity>(EntityType type)
            where TEntity : IDEntity
        {
            var result = new List<AvailableEquipmentItem>();
            var seenIds = new HashSet<int>(); // O(1) deduplication

            // Add vanilla entities first
            if (VanillaLoader.Vanilla?.Database.TryGetValue(type, out var vanillaSet) == true)
            {
                foreach (var entity in vanillaSet.GetFullList())
                {
                    if (entity is TEntity typedEntity && seenIds.Add(typedEntity.ID))
                    {
                        result.Add(new AvailableEquipmentItem
                        {
                            ID = typedEntity.ID,
                            Name = typedEntity.Name,
                            Source = "Vanilla"
                        });
                    }
                }
            }

            // Add mod entities (new ones only, updates use vanilla entry)
            if (_mod?.Database.TryGetValue(type, out var modSet) == true)
            {
                foreach (var entity in modSet.GetFullList())
                {
                    if (entity is TEntity typedEntity)
                    {
                        if (seenIds.Add(typedEntity.ID))
                        {
                            // New mod entity
                            result.Add(new AvailableEquipmentItem
                            {
                                ID = typedEntity.ID,
                                Name = typedEntity.Name,
                                Source = "Mod"
                            });
                        }
                        else
                        {
                            // Mod override of vanilla - update the name if different
                            var existing = result.FirstOrDefault(r => r.ID == typedEntity.ID);
                            if (existing != null && !string.IsNullOrEmpty(typedEntity.Name))
                            {
                                existing.Name = typedEntity.Name;
                                existing.Source = "Mod"; // Mark as modified by mod
                            }
                        }
                    }
                }
            }

            // Sort by ID for consistent display
            result.Sort((a, b) => a.ID.CompareTo(b.ID));
            return result.AsReadOnly();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
