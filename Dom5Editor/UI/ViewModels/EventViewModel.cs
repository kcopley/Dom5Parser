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
    /// ViewModel for Event entities.
    /// </summary>
    public class EventViewModel : EntityViewModel
    {
        public EventViewModel(Event entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Event Event => (Event)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from event_badges.json.
        /// </summary>
        protected override string EntityTypeName => "event";

        // ========================================
        // Header Display Properties
        // ========================================

        /// <summary>
        /// Gets the event rarity display for the header.
        /// </summary>
        public string RarityDisplay
        {
            get
            {
                var rarity = GetIntProperty(Command.RARITY);
                return rarity switch
                {
                    0 => "Common",
                    1 => "Uncommon",
                    2 => "Rare",
                    _ => rarity?.ToString() ?? "-"
                };
            }
        }

        // ========================================
        // Badge Collections - Requirements
        // ========================================

        // General Requirements
        private ObservableCollection<PropertyItem> _generalRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableGeneralRequirementsBadges;

        public ObservableCollection<PropertyItem> GeneralRequirementsBadges
        {
            get { if (_generalRequirementsBadges == null) RefreshGeneralRequirementsBadges(); return _generalRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableGeneralRequirementsBadges
        {
            get { if (_availableGeneralRequirementsBadges == null) RefreshGeneralRequirementsBadges(); return _availableGeneralRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeGeneralRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addGeneralRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveGeneralRequirementsBadgeCommand => _removeGeneralRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshGeneralRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddGeneralRequirementsBadgeCommand => _addGeneralRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshGeneralRequirementsBadges);

        private void RefreshGeneralRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("general_requirements", BadgeValueChangedHandler);
            _generalRequirementsBadges = active;
            _availableGeneralRequirementsBadges = available;
            OnPropertyChanged(nameof(GeneralRequirementsBadges));
            OnPropertyChanged(nameof(AvailableGeneralRequirementsBadges));
        }

        // Nation Requirements
        private ObservableCollection<PropertyItem> _nationRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableNationRequirementsBadges;

        public ObservableCollection<PropertyItem> NationRequirementsBadges
        {
            get { if (_nationRequirementsBadges == null) RefreshNationRequirementsBadges(); return _nationRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableNationRequirementsBadges
        {
            get { if (_availableNationRequirementsBadges == null) RefreshNationRequirementsBadges(); return _availableNationRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeNationRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addNationRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveNationRequirementsBadgeCommand => _removeNationRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshNationRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddNationRequirementsBadgeCommand => _addNationRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshNationRequirementsBadges);

        private void RefreshNationRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("nation_requirements", BadgeValueChangedHandler);
            _nationRequirementsBadges = active;
            _availableNationRequirementsBadges = available;
            OnPropertyChanged(nameof(NationRequirementsBadges));
            OnPropertyChanged(nameof(AvailableNationRequirementsBadges));
        }

        // Location Requirements
        private ObservableCollection<PropertyItem> _locationRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableLocationRequirementsBadges;

        public ObservableCollection<PropertyItem> LocationRequirementsBadges
        {
            get { if (_locationRequirementsBadges == null) RefreshLocationRequirementsBadges(); return _locationRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableLocationRequirementsBadges
        {
            get { if (_availableLocationRequirementsBadges == null) RefreshLocationRequirementsBadges(); return _availableLocationRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeLocationRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addLocationRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveLocationRequirementsBadgeCommand => _removeLocationRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshLocationRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddLocationRequirementsBadgeCommand => _addLocationRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshLocationRequirementsBadges);

        private void RefreshLocationRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("location_requirements", BadgeValueChangedHandler);
            _locationRequirementsBadges = active;
            _availableLocationRequirementsBadges = available;
            OnPropertyChanged(nameof(LocationRequirementsBadges));
            OnPropertyChanged(nameof(AvailableLocationRequirementsBadges));
        }

        // Province Requirements
        private ObservableCollection<PropertyItem> _provinceRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableProvinceRequirementsBadges;

        public ObservableCollection<PropertyItem> ProvinceRequirementsBadges
        {
            get { if (_provinceRequirementsBadges == null) RefreshProvinceRequirementsBadges(); return _provinceRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableProvinceRequirementsBadges
        {
            get { if (_availableProvinceRequirementsBadges == null) RefreshProvinceRequirementsBadges(); return _availableProvinceRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeProvinceRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addProvinceRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveProvinceRequirementsBadgeCommand => _removeProvinceRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshProvinceRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddProvinceRequirementsBadgeCommand => _addProvinceRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshProvinceRequirementsBadges);

        private void RefreshProvinceRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("province_requirements", BadgeValueChangedHandler);
            _provinceRequirementsBadges = active;
            _availableProvinceRequirementsBadges = available;
            OnPropertyChanged(nameof(ProvinceRequirementsBadges));
            OnPropertyChanged(nameof(AvailableProvinceRequirementsBadges));
        }

        // Site Requirements
        private ObservableCollection<PropertyItem> _siteRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableSiteRequirementsBadges;

        public ObservableCollection<PropertyItem> SiteRequirementsBadges
        {
            get { if (_siteRequirementsBadges == null) RefreshSiteRequirementsBadges(); return _siteRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableSiteRequirementsBadges
        {
            get { if (_availableSiteRequirementsBadges == null) RefreshSiteRequirementsBadges(); return _availableSiteRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeSiteRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addSiteRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveSiteRequirementsBadgeCommand => _removeSiteRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshSiteRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddSiteRequirementsBadgeCommand => _addSiteRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshSiteRequirementsBadges);

        private void RefreshSiteRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("site_requirements", BadgeValueChangedHandler);
            _siteRequirementsBadges = active;
            _availableSiteRequirementsBadges = available;
            OnPropertyChanged(nameof(SiteRequirementsBadges));
            OnPropertyChanged(nameof(AvailableSiteRequirementsBadges));
        }

        // Dominion Requirements
        private ObservableCollection<PropertyItem> _dominionRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableDominionRequirementsBadges;

        public ObservableCollection<PropertyItem> DominionRequirementsBadges
        {
            get { if (_dominionRequirementsBadges == null) RefreshDominionRequirementsBadges(); return _dominionRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableDominionRequirementsBadges
        {
            get { if (_availableDominionRequirementsBadges == null) RefreshDominionRequirementsBadges(); return _availableDominionRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeDominionRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addDominionRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveDominionRequirementsBadgeCommand => _removeDominionRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshDominionRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddDominionRequirementsBadgeCommand => _addDominionRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshDominionRequirementsBadges);

        private void RefreshDominionRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("dominion_requirements", BadgeValueChangedHandler);
            _dominionRequirementsBadges = active;
            _availableDominionRequirementsBadges = available;
            OnPropertyChanged(nameof(DominionRequirementsBadges));
            OnPropertyChanged(nameof(AvailableDominionRequirementsBadges));
        }

        // Path Requirements
        private ObservableCollection<PropertyItem> _pathRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePathRequirementsBadges;

        public ObservableCollection<PropertyItem> PathRequirementsBadges
        {
            get { if (_pathRequirementsBadges == null) RefreshPathRequirementsBadges(); return _pathRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePathRequirementsBadges
        {
            get { if (_availablePathRequirementsBadges == null) RefreshPathRequirementsBadges(); return _availablePathRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removePathRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPathRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemovePathRequirementsBadgeCommand => _removePathRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPathRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddPathRequirementsBadgeCommand => _addPathRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshPathRequirementsBadges);

        private void RefreshPathRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("path_requirements", BadgeValueChangedHandler);
            _pathRequirementsBadges = active;
            _availablePathRequirementsBadges = available;
            OnPropertyChanged(nameof(PathRequirementsBadges));
            OnPropertyChanged(nameof(AvailablePathRequirementsBadges));
        }

        // Commander Requirements
        private ObservableCollection<PropertyItem> _commanderRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCommanderRequirementsBadges;

        public ObservableCollection<PropertyItem> CommanderRequirementsBadges
        {
            get { if (_commanderRequirementsBadges == null) RefreshCommanderRequirementsBadges(); return _commanderRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCommanderRequirementsBadges
        {
            get { if (_availableCommanderRequirementsBadges == null) RefreshCommanderRequirementsBadges(); return _availableCommanderRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeCommanderRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCommanderRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveCommanderRequirementsBadgeCommand => _removeCommanderRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCommanderRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddCommanderRequirementsBadgeCommand => _addCommanderRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshCommanderRequirementsBadges);

        private void RefreshCommanderRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("commander_requirements", BadgeValueChangedHandler);
            _commanderRequirementsBadges = active;
            _availableCommanderRequirementsBadges = available;
            OnPropertyChanged(nameof(CommanderRequirementsBadges));
            OnPropertyChanged(nameof(AvailableCommanderRequirementsBadges));
        }

        // Target Requirements
        private ObservableCollection<PropertyItem> _targetRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableTargetRequirementsBadges;

        public ObservableCollection<PropertyItem> TargetRequirementsBadges
        {
            get { if (_targetRequirementsBadges == null) RefreshTargetRequirementsBadges(); return _targetRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableTargetRequirementsBadges
        {
            get { if (_availableTargetRequirementsBadges == null) RefreshTargetRequirementsBadges(); return _availableTargetRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeTargetRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addTargetRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveTargetRequirementsBadgeCommand => _removeTargetRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshTargetRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddTargetRequirementsBadgeCommand => _addTargetRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshTargetRequirementsBadges);

        private void RefreshTargetRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("target_requirements", BadgeValueChangedHandler);
            _targetRequirementsBadges = active;
            _availableTargetRequirementsBadges = available;
            OnPropertyChanged(nameof(TargetRequirementsBadges));
            OnPropertyChanged(nameof(AvailableTargetRequirementsBadges));
        }

        // Code Requirements
        private ObservableCollection<PropertyItem> _codeRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableCodeRequirementsBadges;

        public ObservableCollection<PropertyItem> CodeRequirementsBadges
        {
            get { if (_codeRequirementsBadges == null) RefreshCodeRequirementsBadges(); return _codeRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableCodeRequirementsBadges
        {
            get { if (_availableCodeRequirementsBadges == null) RefreshCodeRequirementsBadges(); return _availableCodeRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeCodeRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addCodeRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveCodeRequirementsBadgeCommand => _removeCodeRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshCodeRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddCodeRequirementsBadgeCommand => _addCodeRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshCodeRequirementsBadges);

        private void RefreshCodeRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("code_requirements", BadgeValueChangedHandler);
            _codeRequirementsBadges = active;
            _availableCodeRequirementsBadges = available;
            OnPropertyChanged(nameof(CodeRequirementsBadges));
            OnPropertyChanged(nameof(AvailableCodeRequirementsBadges));
        }

        // Enchantment Requirements
        private ObservableCollection<PropertyItem> _enchantmentRequirementsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEnchantmentRequirementsBadges;

        public ObservableCollection<PropertyItem> EnchantmentRequirementsBadges
        {
            get { if (_enchantmentRequirementsBadges == null) RefreshEnchantmentRequirementsBadges(); return _enchantmentRequirementsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEnchantmentRequirementsBadges
        {
            get { if (_availableEnchantmentRequirementsBadges == null) RefreshEnchantmentRequirementsBadges(); return _availableEnchantmentRequirementsBadges; }
        }

        private RelayCommand<PropertyItem> _removeEnchantmentRequirementsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEnchantmentRequirementsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveEnchantmentRequirementsBadgeCommand => _removeEnchantmentRequirementsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEnchantmentRequirementsBadges);
        public RelayCommand<AvailablePropertyItem> AddEnchantmentRequirementsBadgeCommand => _addEnchantmentRequirementsBadgeCommand ??= CreateAddBadgeCommand(RefreshEnchantmentRequirementsBadges);

        private void RefreshEnchantmentRequirementsBadges()
        {
            var (active, available) = BuildBadgesFromSection("enchantment_requirements", BadgeValueChangedHandler);
            _enchantmentRequirementsBadges = active;
            _availableEnchantmentRequirementsBadges = available;
            OnPropertyChanged(nameof(EnchantmentRequirementsBadges));
            OnPropertyChanged(nameof(AvailableEnchantmentRequirementsBadges));
        }

        // ========================================
        // Badge Collections - Effects
        // ========================================

        // Message
        private ObservableCollection<PropertyItem> _messageBadges;
        private ObservableCollection<AvailablePropertyItem> _availableMessageBadges;

        public ObservableCollection<PropertyItem> MessageBadges
        {
            get { if (_messageBadges == null) RefreshMessageBadges(); return _messageBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableMessageBadges
        {
            get { if (_availableMessageBadges == null) RefreshMessageBadges(); return _availableMessageBadges; }
        }

        private RelayCommand<PropertyItem> _removeMessageBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addMessageBadgeCommand;
        public RelayCommand<PropertyItem> RemoveMessageBadgeCommand => _removeMessageBadgeCommand ??= CreateRemoveBadgeCommand(RefreshMessageBadges);
        public RelayCommand<AvailablePropertyItem> AddMessageBadgeCommand => _addMessageBadgeCommand ??= CreateAddBadgeCommand(RefreshMessageBadges);

        private void RefreshMessageBadges()
        {
            var (active, available) = BuildBadgesFromSection("message", BadgeValueChangedHandler);
            _messageBadges = active;
            _availableMessageBadges = available;
            OnPropertyChanged(nameof(MessageBadges));
            OnPropertyChanged(nameof(AvailableMessageBadges));
        }

        // Resource Effects
        private ObservableCollection<PropertyItem> _resourceEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableResourceEffectsBadges;

        public ObservableCollection<PropertyItem> ResourceEffectsBadges
        {
            get { if (_resourceEffectsBadges == null) RefreshResourceEffectsBadges(); return _resourceEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableResourceEffectsBadges
        {
            get { if (_availableResourceEffectsBadges == null) RefreshResourceEffectsBadges(); return _availableResourceEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeResourceEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addResourceEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveResourceEffectsBadgeCommand => _removeResourceEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshResourceEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddResourceEffectsBadgeCommand => _addResourceEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshResourceEffectsBadges);

        private void RefreshResourceEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("resource_effects", BadgeValueChangedHandler);
            _resourceEffectsBadges = active;
            _availableResourceEffectsBadges = available;
            OnPropertyChanged(nameof(ResourceEffectsBadges));
            OnPropertyChanged(nameof(AvailableResourceEffectsBadges));
        }

        // Province Effects
        private ObservableCollection<PropertyItem> _provinceEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableProvinceEffectsBadges;

        public ObservableCollection<PropertyItem> ProvinceEffectsBadges
        {
            get { if (_provinceEffectsBadges == null) RefreshProvinceEffectsBadges(); return _provinceEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableProvinceEffectsBadges
        {
            get { if (_availableProvinceEffectsBadges == null) RefreshProvinceEffectsBadges(); return _availableProvinceEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeProvinceEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addProvinceEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveProvinceEffectsBadgeCommand => _removeProvinceEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshProvinceEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddProvinceEffectsBadgeCommand => _addProvinceEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshProvinceEffectsBadges);

        private void RefreshProvinceEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("province_effects", BadgeValueChangedHandler);
            _provinceEffectsBadges = active;
            _availableProvinceEffectsBadges = available;
            OnPropertyChanged(nameof(ProvinceEffectsBadges));
            OnPropertyChanged(nameof(AvailableProvinceEffectsBadges));
        }

        // Scale Effects
        private ObservableCollection<PropertyItem> _scaleEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableScaleEffectsBadges;

        public ObservableCollection<PropertyItem> ScaleEffectsBadges
        {
            get { if (_scaleEffectsBadges == null) RefreshScaleEffectsBadges(); return _scaleEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableScaleEffectsBadges
        {
            get { if (_availableScaleEffectsBadges == null) RefreshScaleEffectsBadges(); return _availableScaleEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeScaleEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addScaleEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveScaleEffectsBadgeCommand => _removeScaleEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshScaleEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddScaleEffectsBadgeCommand => _addScaleEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshScaleEffectsBadges);

        private void RefreshScaleEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("scale_effects", BadgeValueChangedHandler);
            _scaleEffectsBadges = active;
            _availableScaleEffectsBadges = available;
            OnPropertyChanged(nameof(ScaleEffectsBadges));
            OnPropertyChanged(nameof(AvailableScaleEffectsBadges));
        }

        // Unit Spawn
        private ObservableCollection<PropertyItem> _unitSpawnBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUnitSpawnBadges;

        public ObservableCollection<PropertyItem> UnitSpawnBadges
        {
            get { if (_unitSpawnBadges == null) RefreshUnitSpawnBadges(); return _unitSpawnBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUnitSpawnBadges
        {
            get { if (_availableUnitSpawnBadges == null) RefreshUnitSpawnBadges(); return _availableUnitSpawnBadges; }
        }

        private RelayCommand<PropertyItem> _removeUnitSpawnBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUnitSpawnBadgeCommand;
        public RelayCommand<PropertyItem> RemoveUnitSpawnBadgeCommand => _removeUnitSpawnBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUnitSpawnBadges);
        public RelayCommand<AvailablePropertyItem> AddUnitSpawnBadgeCommand => _addUnitSpawnBadgeCommand ??= CreateAddBadgeCommand(RefreshUnitSpawnBadges);

        private void RefreshUnitSpawnBadges()
        {
            var (active, available) = BuildBadgesFromSection("unit_spawn", BadgeValueChangedHandler);
            _unitSpawnBadges = active;
            _availableUnitSpawnBadges = available;
            OnPropertyChanged(nameof(UnitSpawnBadges));
            OnPropertyChanged(nameof(AvailableUnitSpawnBadges));
        }

        // Unit Effects
        private ObservableCollection<PropertyItem> _unitEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUnitEffectsBadges;

        public ObservableCollection<PropertyItem> UnitEffectsBadges
        {
            get { if (_unitEffectsBadges == null) RefreshUnitEffectsBadges(); return _unitEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUnitEffectsBadges
        {
            get { if (_availableUnitEffectsBadges == null) RefreshUnitEffectsBadges(); return _availableUnitEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeUnitEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUnitEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveUnitEffectsBadgeCommand => _removeUnitEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUnitEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddUnitEffectsBadgeCommand => _addUnitEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshUnitEffectsBadges);

        private void RefreshUnitEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("unit_effects", BadgeValueChangedHandler);
            _unitEffectsBadges = active;
            _availableUnitEffectsBadges = available;
            OnPropertyChanged(nameof(UnitEffectsBadges));
            OnPropertyChanged(nameof(AvailableUnitEffectsBadges));
        }

        // Path Boost
        private ObservableCollection<PropertyItem> _pathBoostBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePathBoostBadges;

        public ObservableCollection<PropertyItem> PathBoostBadges
        {
            get { if (_pathBoostBadges == null) RefreshPathBoostBadges(); return _pathBoostBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePathBoostBadges
        {
            get { if (_availablePathBoostBadges == null) RefreshPathBoostBadges(); return _availablePathBoostBadges; }
        }

        private RelayCommand<PropertyItem> _removePathBoostBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPathBoostBadgeCommand;
        public RelayCommand<PropertyItem> RemovePathBoostBadgeCommand => _removePathBoostBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPathBoostBadges);
        public RelayCommand<AvailablePropertyItem> AddPathBoostBadgeCommand => _addPathBoostBadgeCommand ??= CreateAddBadgeCommand(RefreshPathBoostBadges);

        private void RefreshPathBoostBadges()
        {
            var (active, available) = BuildBadgesFromSection("path_boost", BadgeValueChangedHandler);
            _pathBoostBadges = active;
            _availablePathBoostBadges = available;
            OnPropertyChanged(nameof(PathBoostBadges));
            OnPropertyChanged(nameof(AvailablePathBoostBadges));
        }

        // World Effects
        private ObservableCollection<PropertyItem> _worldEffectsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableWorldEffectsBadges;

        public ObservableCollection<PropertyItem> WorldEffectsBadges
        {
            get { if (_worldEffectsBadges == null) RefreshWorldEffectsBadges(); return _worldEffectsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableWorldEffectsBadges
        {
            get { if (_availableWorldEffectsBadges == null) RefreshWorldEffectsBadges(); return _availableWorldEffectsBadges; }
        }

        private RelayCommand<PropertyItem> _removeWorldEffectsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addWorldEffectsBadgeCommand;
        public RelayCommand<PropertyItem> RemoveWorldEffectsBadgeCommand => _removeWorldEffectsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshWorldEffectsBadges);
        public RelayCommand<AvailablePropertyItem> AddWorldEffectsBadgeCommand => _addWorldEffectsBadgeCommand ??= CreateAddBadgeCommand(RefreshWorldEffectsBadges);

        private void RefreshWorldEffectsBadges()
        {
            var (active, available) = BuildBadgesFromSection("world_effects", BadgeValueChangedHandler);
            _worldEffectsBadges = active;
            _availableWorldEffectsBadges = available;
            OnPropertyChanged(nameof(WorldEffectsBadges));
            OnPropertyChanged(nameof(AvailableWorldEffectsBadges));
        }

        // Event Control
        private ObservableCollection<PropertyItem> _eventControlBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEventControlBadges;

        public ObservableCollection<PropertyItem> EventControlBadges
        {
            get { if (_eventControlBadges == null) RefreshEventControlBadges(); return _eventControlBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEventControlBadges
        {
            get { if (_availableEventControlBadges == null) RefreshEventControlBadges(); return _availableEventControlBadges; }
        }

        private RelayCommand<PropertyItem> _removeEventControlBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEventControlBadgeCommand;
        public RelayCommand<PropertyItem> RemoveEventControlBadgeCommand => _removeEventControlBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEventControlBadges);
        public RelayCommand<AvailablePropertyItem> AddEventControlBadgeCommand => _addEventControlBadgeCommand ??= CreateAddBadgeCommand(RefreshEventControlBadges);

        private void RefreshEventControlBadges()
        {
            var (active, available) = BuildBadgesFromSection("event_control", BadgeValueChangedHandler);
            _eventControlBadges = active;
            _availableEventControlBadges = available;
            OnPropertyChanged(nameof(EventControlBadges));
            OnPropertyChanged(nameof(AvailableEventControlBadges));
        }

        // ========================================
        // Shared Badge Value Changed Handler
        // ========================================

        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh all badge collections on any property change
            // This ensures undo/redo properly updates the UI
            RefreshAllBadgeCollections();

            // Update header display if rarity changes
            if (command == Command.RARITY)
            {
                OnPropertyChanged(nameof(RarityDisplay));
            }
        }

        /// <summary>
        /// Refreshes all badge collections. Called on undo/redo operations.
        /// </summary>
        private void RefreshAllBadgeCollections()
        {
            // Requirements
            RefreshGeneralRequirementsBadges();
            RefreshNationRequirementsBadges();
            RefreshLocationRequirementsBadges();
            RefreshProvinceRequirementsBadges();
            RefreshSiteRequirementsBadges();
            RefreshDominionRequirementsBadges();
            RefreshPathRequirementsBadges();
            RefreshCommanderRequirementsBadges();
            RefreshTargetRequirementsBadges();
            RefreshCodeRequirementsBadges();
            RefreshEnchantmentRequirementsBadges();

            // Effects
            RefreshMessageBadges();
            RefreshResourceEffectsBadges();
            RefreshProvinceEffectsBadges();
            RefreshScaleEffectsBadges();
            RefreshUnitSpawnBadges();
            RefreshUnitEffectsBadges();
            RefreshPathBoostBadges();
            RefreshWorldEffectsBadges();
            RefreshEventControlBadges();
        }
    }
}
