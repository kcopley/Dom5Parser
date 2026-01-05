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

        public void Validate()
        {
            if (_mod == null) return;

            var validator = new ModValidator();
            var results = validator.ValidateWithSummary(_mod);

            StatusMessage = $"Validation: {results.ErrorCount} errors, {results.WarningCount} warnings";

            // TODO: Open validation panel with full results
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

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
