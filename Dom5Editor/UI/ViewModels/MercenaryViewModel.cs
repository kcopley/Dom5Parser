using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for Mercenary entities.
    /// All properties are now JSON-driven via badge panels.
    /// </summary>
    public class MercenaryViewModel : EntityViewModel
    {
        public MercenaryViewModel(Mercenary entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Mercenary Mercenary => (Mercenary)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from mercenary_badges.json.
        /// </summary>
        protected override string EntityTypeName => "mercenary";

        // ========================================
        // Derived Display Properties (kept for header)
        // ========================================

        /// <summary>
        /// Gets the era display string from the era bitmask.
        /// </summary>
        public string EraDisplay
        {
            get
            {
                var mask = GetIntProperty(Command.ERAMASK) ?? 0;
                if (mask == 0) return "None";
                var eras = new List<string>();
                if ((mask & 1) != 0) eras.Add("EA");
                if ((mask & 2) != 0) eras.Add("MA");
                if ((mask & 4) != 0) eras.Add("LA");
                return string.Join("/", eras);
            }
        }

        // ========================================
        // Badge Collections
        // ========================================

        private ObservableCollection<PropertyItem> _identityBadges;
        private ObservableCollection<AvailablePropertyItem> _availableIdentityBadges;
        private ObservableCollection<PropertyItem> _unitsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableUnitsBadges;
        private ObservableCollection<PropertyItem> _economicsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEconomicsBadges;
        private ObservableCollection<PropertyItem> _equipmentBadges;
        private ObservableCollection<AvailablePropertyItem> _availableEquipmentBadges;

        public ObservableCollection<PropertyItem> IdentityBadges
        {
            get { if (_identityBadges == null) RefreshIdentityBadges(); return _identityBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableIdentityBadges
        {
            get { if (_availableIdentityBadges == null) RefreshIdentityBadges(); return _availableIdentityBadges; }
        }

        public ObservableCollection<PropertyItem> UnitsBadges
        {
            get { if (_unitsBadges == null) RefreshUnitsBadges(); return _unitsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableUnitsBadges
        {
            get { if (_availableUnitsBadges == null) RefreshUnitsBadges(); return _availableUnitsBadges; }
        }

        public ObservableCollection<PropertyItem> EconomicsBadges
        {
            get { if (_economicsBadges == null) RefreshEconomicsBadges(); return _economicsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEconomicsBadges
        {
            get { if (_availableEconomicsBadges == null) RefreshEconomicsBadges(); return _availableEconomicsBadges; }
        }

        public ObservableCollection<PropertyItem> EquipmentBadges
        {
            get { if (_equipmentBadges == null) RefreshEquipmentBadges(); return _equipmentBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableEquipmentBadges
        {
            get { if (_availableEquipmentBadges == null) RefreshEquipmentBadges(); return _availableEquipmentBadges; }
        }

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removeIdentityBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addIdentityBadgeCommand;
        private RelayCommand<PropertyItem> _removeUnitsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addUnitsBadgeCommand;
        private RelayCommand<PropertyItem> _removeEconomicsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEconomicsBadgeCommand;
        private RelayCommand<PropertyItem> _removeEquipmentBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addEquipmentBadgeCommand;

        public RelayCommand<PropertyItem> RemoveIdentityBadgeCommand => _removeIdentityBadgeCommand ??= CreateRemoveBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<AvailablePropertyItem> AddIdentityBadgeCommand => _addIdentityBadgeCommand ??= CreateAddBadgeCommand(RefreshIdentityBadges);
        public RelayCommand<PropertyItem> RemoveUnitsBadgeCommand => _removeUnitsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshUnitsBadges);
        public RelayCommand<AvailablePropertyItem> AddUnitsBadgeCommand => _addUnitsBadgeCommand ??= CreateAddBadgeCommand(RefreshUnitsBadges);
        public RelayCommand<PropertyItem> RemoveEconomicsBadgeCommand => _removeEconomicsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEconomicsBadges);
        public RelayCommand<AvailablePropertyItem> AddEconomicsBadgeCommand => _addEconomicsBadgeCommand ??= CreateAddBadgeCommand(RefreshEconomicsBadges);
        public RelayCommand<PropertyItem> RemoveEquipmentBadgeCommand => _removeEquipmentBadgeCommand ??= CreateRemoveBadgeCommand(RefreshEquipmentBadges);
        public RelayCommand<AvailablePropertyItem> AddEquipmentBadgeCommand => _addEquipmentBadgeCommand ??= CreateAddBadgeCommand(RefreshEquipmentBadges);

        // Shared value changed handler
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshIdentityBadges()
        {
            var (active, available) = BuildBadgesFromSection("identity", BadgeValueChangedHandler);
            _identityBadges = active;
            _availableIdentityBadges = available;
            OnPropertyChanged(nameof(IdentityBadges));
            OnPropertyChanged(nameof(AvailableIdentityBadges));
            // EraDisplay depends on eramask badge value
            OnPropertyChanged(nameof(EraDisplay));
        }

        private void RefreshUnitsBadges()
        {
            var (active, available) = BuildBadgesFromSection("units", BadgeValueChangedHandler);
            _unitsBadges = active;
            _availableUnitsBadges = available;
            OnPropertyChanged(nameof(UnitsBadges));
            OnPropertyChanged(nameof(AvailableUnitsBadges));
        }

        private void RefreshEconomicsBadges()
        {
            var (active, available) = BuildBadgesFromSection("economics", BadgeValueChangedHandler);
            _economicsBadges = active;
            _availableEconomicsBadges = available;
            OnPropertyChanged(nameof(EconomicsBadges));
            OnPropertyChanged(nameof(AvailableEconomicsBadges));
        }

        private void RefreshEquipmentBadges()
        {
            var (active, available) = BuildBadgesFromSection("equipment", BadgeValueChangedHandler);
            _equipmentBadges = active;
            _availableEquipmentBadges = available;
            OnPropertyChanged(nameof(EquipmentBadges));
            OnPropertyChanged(nameof(AvailableEquipmentBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh all badge collections on undo/redo
            RefreshIdentityBadges();
            RefreshUnitsBadges();
            RefreshEconomicsBadges();
            RefreshEquipmentBadges();
        }
    }
}
