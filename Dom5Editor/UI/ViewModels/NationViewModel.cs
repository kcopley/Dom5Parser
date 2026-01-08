using System;
using System.Collections.ObjectModel;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for Nation entities.
    /// Uses JSON-driven badge panels for property editing.
    /// </summary>
    public class NationViewModel : EntityViewModel
    {
        public NationViewModel(Nation entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Nation Nation => (Nation)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from nation_badges.json.
        /// </summary>
        protected override string EntityTypeName => "nation";

        // ========================================
        // Computed Display Properties
        // ========================================

        /// <summary>
        /// Gets the era display name for header display.
        /// Derived from the era property value.
        /// </summary>
        public string EraDisplay
        {
            get
            {
                var era = GetIntProperty(Command.ERA);
                return era switch
                {
                    1 => "Early Age",
                    2 => "Middle Age",
                    3 => "Late Age",
                    _ => era?.ToString() ?? "-"
                };
            }
        }

        /// <summary>
        /// Gets the nation epithet for header display.
        /// </summary>
        public string Epithet => GetStringProperty(Command.EPITHET);

        // ========================================
        // Data Availability Indicators
        // ========================================

        /// <summary>
        /// Returns true if this is a vanilla nation (may have incomplete data).
        /// </summary>
        public bool IsVanillaNation => Source == EntitySource.Vanilla;

        /// <summary>
        /// Returns true if this is a mod-defined nation (complete data available).
        /// </summary>
        public bool IsModNation => Source == EntitySource.FromMod || Source == EntitySource.VanillaModified;

        /// <summary>
        /// Gets a message about data availability for vanilla nations.
        /// </summary>
        public string DataAvailabilityMessage
        {
            get
            {
                if (IsVanillaNation)
                    return "Some data may not be displayed. Vanilla nation data is incomplete - only mod-defined properties can be shown.";
                return null;
            }
        }

        /// <summary>
        /// Returns true if the data availability warning should be shown.
        /// </summary>
        public bool ShowDataWarning => IsVanillaNation;

        // ========================================
        // Badge Collections - Identity
        // ========================================

        private ObservableCollection<PropertyItem> _identityBadges;
        private ObservableCollection<AvailablePropertyItem> _availableIdentityBadges;

        public ObservableCollection<PropertyItem> IdentityBadges
        {
            get { if (_identityBadges == null) RefreshIdentityBadges(); return _identityBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableIdentityBadges
        {
            get { if (_availableIdentityBadges == null) RefreshIdentityBadges(); return _availableIdentityBadges; }
        }

        private RelayCommand<PropertyItem> _removeIdentityBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addIdentityBadgeCommand;
        public RelayCommand<PropertyItem> RemoveIdentityBadgeCommand => _removeIdentityBadgeCommand ??= CreateRemoveBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<AvailablePropertyItem> AddIdentityBadgeCommand => _addIdentityBadgeCommand ??= CreateAddBadgeCommand(RefreshIdentityBadges);

        private void RefreshIdentityBadges()
        {
            var (active, available) = BuildBadgesFromSection("identity", BadgeValueChangedHandler);
            _identityBadges = active;
            _availableIdentityBadges = available;
            OnPropertyChanged(nameof(IdentityBadges));
            OnPropertyChanged(nameof(AvailableIdentityBadges));
        }

        // ========================================
        // Badge Collections - Recruitment
        // ========================================

        private ObservableCollection<PropertyItem> _recruitmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableRecruitmentBadges;

        public ObservableCollection<PropertyItem> RecruitmentBadges
        {
            get { if (_recruitmentBadges == null) RefreshRecruitmentBadges(); return _recruitmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableRecruitmentBadges
        {
            get { if (_availableRecruitmentBadges == null) RefreshRecruitmentBadges(); return _availableRecruitmentBadges; }
        }

        private RelayCommand<PropertyItem> _removeRecruitmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addRecruitmentBadgeCommand;
        public RelayCommand<PropertyItem> RemoveRecruitmentBadgeCommand => _removeRecruitmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshRecruitmentBadges);
        public RelayCommand<AvailablePropertyItem> AddRecruitmentBadgeCommand => _addRecruitmentBadgeCommand ??= CreateAddBadgeCommand(RefreshRecruitmentBadges);

        private void RefreshRecruitmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("recruitment", BadgeValueChangedHandler);
            _recruitmentBadges = active;
            _availableRecruitmentBadges = available;
            OnPropertyChanged(nameof(RecruitmentBadges));
            OnPropertyChanged(nameof(AvailableRecruitmentBadges));
        }

        // ========================================
        // Badge Collections - Terrain Recruitment
        // ========================================

        private ObservableCollection<PropertyItem> _terrainRecruitmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableTerrainRecruitmentBadges;

        public ObservableCollection<PropertyItem> TerrainRecruitmentBadges
        {
            get { if (_terrainRecruitmentBadges == null) RefreshTerrainRecruitmentBadges(); return _terrainRecruitmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableTerrainRecruitmentBadges
        {
            get { if (_availableTerrainRecruitmentBadges == null) RefreshTerrainRecruitmentBadges(); return _availableTerrainRecruitmentBadges; }
        }

        private RelayCommand<PropertyItem> _removeTerrainRecruitmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addTerrainRecruitmentBadgeCommand;
        public RelayCommand<PropertyItem> RemoveTerrainRecruitmentBadgeCommand => _removeTerrainRecruitmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshTerrainRecruitmentBadges);
        public RelayCommand<AvailablePropertyItem> AddTerrainRecruitmentBadgeCommand => _addTerrainRecruitmentBadgeCommand ??= CreateAddBadgeCommand(RefreshTerrainRecruitmentBadges);

        private void RefreshTerrainRecruitmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("terrain_recruitment", BadgeValueChangedHandler);
            _terrainRecruitmentBadges = active;
            _availableTerrainRecruitmentBadges = available;
            OnPropertyChanged(nameof(TerrainRecruitmentBadges));
            OnPropertyChanged(nameof(AvailableTerrainRecruitmentBadges));
        }

        // ========================================
        // Badge Collections - Coastal/UW
        // ========================================

        private ObservableCollection<PropertyItem> _coastalUwBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCoastalUwBadges;

        public ObservableCollection<PropertyItem> CoastalUwBadges
        {
            get { if (_coastalUwBadges == null) RefreshCoastalUwBadges(); return _coastalUwBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCoastalUwBadges
        {
            get { if (_availableCoastalUwBadges == null) RefreshCoastalUwBadges(); return _availableCoastalUwBadges; }
        }

        private RelayCommand<PropertyItem> _removeCoastalUwBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCoastalUwBadgeCommand;
        public RelayCommand<PropertyItem> RemoveCoastalUwBadgeCommand => _removeCoastalUwBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCoastalUwBadges);
        public RelayCommand<AvailablePropertyItem> AddCoastalUwBadgeCommand => _addCoastalUwBadgeCommand ??= CreateAddBadgeCommand(RefreshCoastalUwBadges);

        private void RefreshCoastalUwBadges()
        {
            var (active, available) = BuildBadgesFromSection("coastal_uw", BadgeValueChangedHandler);
            _coastalUwBadges = active;
            _availableCoastalUwBadges = available;
            OnPropertyChanged(nameof(CoastalUwBadges));
            OnPropertyChanged(nameof(AvailableCoastalUwBadges));
        }

        // ========================================
        // Badge Collections - Heroes
        // ========================================

        private ObservableCollection<PropertyItem> _heroesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableHeroesBadges;

        public ObservableCollection<PropertyItem> HeroesBadges
        {
            get { if (_heroesBadges == null) RefreshHeroesBadges(); return _heroesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableHeroesBadges
        {
            get { if (_availableHeroesBadges == null) RefreshHeroesBadges(); return _availableHeroesBadges; }
        }

        private RelayCommand<PropertyItem> _removeHeroesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addHeroesBadgeCommand;
        public RelayCommand<PropertyItem> RemoveHeroesBadgeCommand => _removeHeroesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshHeroesBadges);
        public RelayCommand<AvailablePropertyItem> AddHeroesBadgeCommand => _addHeroesBadgeCommand ??= CreateAddBadgeCommand(RefreshHeroesBadges);

        private void RefreshHeroesBadges()
        {
            var (active, available) = BuildBadgesFromSection("heroes", BadgeValueChangedHandler);
            _heroesBadges = active;
            _availableHeroesBadges = available;
            OnPropertyChanged(nameof(HeroesBadges));
            OnPropertyChanged(nameof(AvailableHeroesBadges));
        }

        // ========================================
        // Badge Collections - Starting Units
        // ========================================

        private ObservableCollection<PropertyItem> _startingBadges;
        private ObservableCollection<AvailablePropertyItem> _availableStartingBadges;

        public ObservableCollection<PropertyItem> StartingBadges
        {
            get { if (_startingBadges == null) RefreshStartingBadges(); return _startingBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableStartingBadges
        {
            get { if (_availableStartingBadges == null) RefreshStartingBadges(); return _availableStartingBadges; }
        }

        private RelayCommand<PropertyItem> _removeStartingBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addStartingBadgeCommand;
        public RelayCommand<PropertyItem> RemoveStartingBadgeCommand => _removeStartingBadgeCommand ??= CreateRemoveBadgeCommand(RefreshStartingBadges);
        public RelayCommand<AvailablePropertyItem> AddStartingBadgeCommand => _addStartingBadgeCommand ??= CreateAddBadgeCommand(RefreshStartingBadges);

        private void RefreshStartingBadges()
        {
            var (active, available) = BuildBadgesFromSection("starting", BadgeValueChangedHandler);
            _startingBadges = active;
            _availableStartingBadges = available;
            OnPropertyChanged(nameof(StartingBadges));
            OnPropertyChanged(nameof(AvailableStartingBadges));
        }

        // ========================================
        // Badge Collections - Province Defense
        // ========================================

        private ObservableCollection<PropertyItem> _provinceDefenseBadges;
        private ObservableCollection<AvailablePropertyItem> _availableProvinceDefenseBadges;

        public ObservableCollection<PropertyItem> ProvinceDefenseBadges
        {
            get { if (_provinceDefenseBadges == null) RefreshProvinceDefenseBadges(); return _provinceDefenseBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableProvinceDefenseBadges
        {
            get { if (_availableProvinceDefenseBadges == null) RefreshProvinceDefenseBadges(); return _availableProvinceDefenseBadges; }
        }

        private RelayCommand<PropertyItem> _removeProvinceDefenseBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addProvinceDefenseBadgeCommand;
        public RelayCommand<PropertyItem> RemoveProvinceDefenseBadgeCommand => _removeProvinceDefenseBadgeCommand ??= CreateRemoveBadgeCommand(RefreshProvinceDefenseBadges);
        public RelayCommand<AvailablePropertyItem> AddProvinceDefenseBadgeCommand => _addProvinceDefenseBadgeCommand ??= CreateAddBadgeCommand(RefreshProvinceDefenseBadges);

        private void RefreshProvinceDefenseBadges()
        {
            var (active, available) = BuildBadgesFromSection("province_defense", BadgeValueChangedHandler);
            _provinceDefenseBadges = active;
            _availableProvinceDefenseBadges = available;
            OnPropertyChanged(nameof(ProvinceDefenseBadges));
            OnPropertyChanged(nameof(AvailableProvinceDefenseBadges));
        }

        // ========================================
        // Badge Collections - UW Defense
        // ========================================

        private ObservableCollection<PropertyItem> _uwDefenseBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUwDefenseBadges;

        public ObservableCollection<PropertyItem> UwDefenseBadges
        {
            get { if (_uwDefenseBadges == null) RefreshUwDefenseBadges(); return _uwDefenseBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUwDefenseBadges
        {
            get { if (_availableUwDefenseBadges == null) RefreshUwDefenseBadges(); return _availableUwDefenseBadges; }
        }

        private RelayCommand<PropertyItem> _removeUwDefenseBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUwDefenseBadgeCommand;
        public RelayCommand<PropertyItem> RemoveUwDefenseBadgeCommand => _removeUwDefenseBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUwDefenseBadges);
        public RelayCommand<AvailablePropertyItem> AddUwDefenseBadgeCommand => _addUwDefenseBadgeCommand ??= CreateAddBadgeCommand(RefreshUwDefenseBadges);

        private void RefreshUwDefenseBadges()
        {
            var (active, available) = BuildBadgesFromSection("uw_defense", BadgeValueChangedHandler);
            _uwDefenseBadges = active;
            _availableUwDefenseBadges = available;
            OnPropertyChanged(nameof(UwDefenseBadges));
            OnPropertyChanged(nameof(AvailableUwDefenseBadges));
        }

        // ========================================
        // Badge Collections - Pretenders
        // ========================================

        private ObservableCollection<PropertyItem> _pretendersBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePretendersBadges;

        public ObservableCollection<PropertyItem> PretendersBadges
        {
            get { if (_pretendersBadges == null) RefreshPretendersBadges(); return _pretendersBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePretendersBadges
        {
            get { if (_availablePretendersBadges == null) RefreshPretendersBadges(); return _availablePretendersBadges; }
        }

        private RelayCommand<PropertyItem> _removePretendersBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPretendersBadgeCommand;
        public RelayCommand<PropertyItem> RemovePretendersBadgeCommand => _removePretendersBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPretendersBadges);
        public RelayCommand<AvailablePropertyItem> AddPretendersBadgeCommand => _addPretendersBadgeCommand ??= CreateAddBadgeCommand(RefreshPretendersBadges);

        private void RefreshPretendersBadges()
        {
            var (active, available) = BuildBadgesFromSection("pretenders", BadgeValueChangedHandler);
            _pretendersBadges = active;
            _availablePretendersBadges = available;
            OnPropertyChanged(nameof(PretendersBadges));
            OnPropertyChanged(nameof(AvailablePretendersBadges));
        }

        // ========================================
        // Badge Collections - Buildings
        // ========================================

        private ObservableCollection<PropertyItem> _buildingsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableBuildingsBadges;

        public ObservableCollection<PropertyItem> BuildingsBadges
        {
            get { if (_buildingsBadges == null) RefreshBuildingsBadges(); return _buildingsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableBuildingsBadges
        {
            get { if (_availableBuildingsBadges == null) RefreshBuildingsBadges(); return _availableBuildingsBadges; }
        }

        private RelayCommand<PropertyItem> _removeBuildingsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addBuildingsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveBuildingsBadgeCommand => _removeBuildingsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshBuildingsBadges);
        public RelayCommand<AvailablePropertyItem> AddBuildingsBadgeCommand => _addBuildingsBadgeCommand ??= CreateAddBadgeCommand(RefreshBuildingsBadges);

        private void RefreshBuildingsBadges()
        {
            var (active, available) = BuildBadgesFromSection("buildings", BadgeValueChangedHandler);
            _buildingsBadges = active;
            _availableBuildingsBadges = available;
            OnPropertyChanged(nameof(BuildingsBadges));
            OnPropertyChanged(nameof(AvailableBuildingsBadges));
        }

        // ========================================
        // Badge Collections - Scales
        // ========================================

        private ObservableCollection<PropertyItem> _scalesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableScalesBadges;

        public ObservableCollection<PropertyItem> ScalesBadges
        {
            get { if (_scalesBadges == null) RefreshScalesBadges(); return _scalesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableScalesBadges
        {
            get { if (_availableScalesBadges == null) RefreshScalesBadges(); return _availableScalesBadges; }
        }

        private RelayCommand<PropertyItem> _removeScalesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addScalesBadgeCommand;
        public RelayCommand<PropertyItem> RemoveScalesBadgeCommand => _removeScalesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshScalesBadges);
        public RelayCommand<AvailablePropertyItem> AddScalesBadgeCommand => _addScalesBadgeCommand ??= CreateAddBadgeCommand(RefreshScalesBadges);

        private void RefreshScalesBadges()
        {
            var (active, available) = BuildBadgesFromSection("scales", BadgeValueChangedHandler);
            _scalesBadges = active;
            _availableScalesBadges = available;
            OnPropertyChanged(nameof(ScalesBadges));
            OnPropertyChanged(nameof(AvailableScalesBadges));
        }

        // ========================================
        // Badge Collections - Special Mechanics
        // ========================================

        private ObservableCollection<PropertyItem> _specialMechanicsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableSpecialMechanicsBadges;

        public ObservableCollection<PropertyItem> SpecialMechanicsBadges
        {
            get { if (_specialMechanicsBadges == null) RefreshSpecialMechanicsBadges(); return _specialMechanicsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableSpecialMechanicsBadges
        {
            get { if (_availableSpecialMechanicsBadges == null) RefreshSpecialMechanicsBadges(); return _availableSpecialMechanicsBadges; }
        }

        private RelayCommand<PropertyItem> _removeSpecialMechanicsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addSpecialMechanicsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveSpecialMechanicsBadgeCommand => _removeSpecialMechanicsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshSpecialMechanicsBadges);
        public RelayCommand<AvailablePropertyItem> AddSpecialMechanicsBadgeCommand => _addSpecialMechanicsBadgeCommand ??= CreateAddBadgeCommand(RefreshSpecialMechanicsBadges);

        private void RefreshSpecialMechanicsBadges()
        {
            var (active, available) = BuildBadgesFromSection("special_mechanics", BadgeValueChangedHandler);
            _specialMechanicsBadges = active;
            _availableSpecialMechanicsBadges = available;
            OnPropertyChanged(nameof(SpecialMechanicsBadges));
            OnPropertyChanged(nameof(AvailableSpecialMechanicsBadges));
        }

        // ========================================
        // Badge Collections - Bless Bonuses
        // ========================================

        private ObservableCollection<PropertyItem> _blessBonusesBadges;
        private ObservableCollection<AvailablePropertyItem> _availableBlessBonusesBadges;

        public ObservableCollection<PropertyItem> BlessBonusesBadges
        {
            get { if (_blessBonusesBadges == null) RefreshBlessBonusesBadges(); return _blessBonusesBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableBlessBonusesBadges
        {
            get { if (_availableBlessBonusesBadges == null) RefreshBlessBonusesBadges(); return _availableBlessBonusesBadges; }
        }

        private RelayCommand<PropertyItem> _removeBlessBonusesBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addBlessBonusesBadgeCommand;
        public RelayCommand<PropertyItem> RemoveBlessBonusesBadgeCommand => _removeBlessBonusesBadgeCommand ??= CreateRemoveBadgeCommand(RefreshBlessBonusesBadges);
        public RelayCommand<AvailablePropertyItem> AddBlessBonusesBadgeCommand => _addBlessBonusesBadgeCommand ??= CreateAddBadgeCommand(RefreshBlessBonusesBadges);

        private void RefreshBlessBonusesBadges()
        {
            var (active, available) = BuildBadgesFromSection("bless_bonuses", BadgeValueChangedHandler);
            _blessBonusesBadges = active;
            _availableBlessBonusesBadges = available;
            OnPropertyChanged(nameof(BlessBonusesBadges));
            OnPropertyChanged(nameof(AvailableBlessBonusesBadges));
        }

        // ========================================
        // Badge Collections - AI Hints
        // ========================================

        private ObservableCollection<PropertyItem> _aiHintsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableAiHintsBadges;

        public ObservableCollection<PropertyItem> AiHintsBadges
        {
            get { if (_aiHintsBadges == null) RefreshAiHintsBadges(); return _aiHintsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableAiHintsBadges
        {
            get { if (_availableAiHintsBadges == null) RefreshAiHintsBadges(); return _availableAiHintsBadges; }
        }

        private RelayCommand<PropertyItem> _removeAiHintsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addAiHintsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveAiHintsBadgeCommand => _removeAiHintsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshAiHintsBadges);
        public RelayCommand<AvailablePropertyItem> AddAiHintsBadgeCommand => _addAiHintsBadgeCommand ??= CreateAddBadgeCommand(RefreshAiHintsBadges);

        private void RefreshAiHintsBadges()
        {
            var (active, available) = BuildBadgesFromSection("ai_hints", BadgeValueChangedHandler);
            _aiHintsBadges = active;
            _availableAiHintsBadges = available;
            OnPropertyChanged(nameof(AiHintsBadges));
            OnPropertyChanged(nameof(AvailableAiHintsBadges));
        }

        // ========================================
        // Badge Collections - Admin
        // ========================================

        private ObservableCollection<PropertyItem> _adminBadges;
        private ObservableCollection<AvailablePropertyItem> _availableAdminBadges;

        public ObservableCollection<PropertyItem> AdminBadges
        {
            get { if (_adminBadges == null) RefreshAdminBadges(); return _adminBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableAdminBadges
        {
            get { if (_availableAdminBadges == null) RefreshAdminBadges(); return _availableAdminBadges; }
        }

        private RelayCommand<PropertyItem> _removeAdminBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addAdminBadgeCommand;
        public RelayCommand<PropertyItem> RemoveAdminBadgeCommand => _removeAdminBadgeCommand ??= CreateRemoveBadgeCommand(RefreshAdminBadges);
        public RelayCommand<AvailablePropertyItem> AddAdminBadgeCommand => _addAdminBadgeCommand ??= CreateAddBadgeCommand(RefreshAdminBadges);

        private void RefreshAdminBadges()
        {
            var (active, available) = BuildBadgesFromSection("admin", BadgeValueChangedHandler);
            _adminBadges = active;
            _availableAdminBadges = available;
            OnPropertyChanged(nameof(AdminBadges));
            OnPropertyChanged(nameof(AvailableAdminBadges));
        }

        // ========================================
        // Shared Badge Value Changed Handler
        // ========================================

        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        /// <summary>
        /// Refreshes badge collections when properties are changed via undo/redo.
        /// Also updates computed display properties that depend on entity values.
        /// </summary>
        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh computed display properties
            if (command == Command.ERA)
            {
                OnPropertyChanged(nameof(EraDisplay));
            }
            else if (command == Command.EPITHET)
            {
                OnPropertyChanged(nameof(Epithet));
            }

            // Refresh all badge collections since any property change could affect them
            RefreshAllBadges();
        }

        /// <summary>
        /// Refreshes all badge collections. Called after undo/redo operations.
        /// </summary>
        private void RefreshAllBadges()
        {
            RefreshIdentityBadges();
            RefreshRecruitmentBadges();
            RefreshTerrainRecruitmentBadges();
            RefreshCoastalUwBadges();
            RefreshHeroesBadges();
            RefreshStartingBadges();
            RefreshProvinceDefenseBadges();
            RefreshUwDefenseBadges();
            RefreshPretendersBadges();
            RefreshBuildingsBadges();
            RefreshScalesBadges();
            RefreshSpecialMechanicsBadges();
            RefreshBlessBonusesBadges();
            RefreshAiHintsBadges();
            RefreshAdminBadges();
        }
    }
}
