using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// ViewModel for Mercenary entities.
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
        // Core Properties
        // ========================================

        public int? Level
        {
            get => GetIntProperty(Command.LEVEL);
            set => SetIntProperty(Command.LEVEL, value);
        }
        public bool IsLevelModified => IsIntPropertyModifiedFromVanilla(Command.LEVEL);
        public bool IsLevelSessionEdit => IsPropertyEditedInSession(Command.LEVEL);

        public string BossName
        {
            get => GetStringProperty(Command.BOSSNAME);
            set => SetStringProperty(Command.BOSSNAME, value);
        }
        public bool IsBossNameModified => IsStringPropertyModifiedFromVanilla(Command.BOSSNAME);
        public bool IsBossNameSessionEdit => IsPropertyEditedInSession(Command.BOSSNAME);

        public int? EraMask
        {
            get => GetIntProperty(Command.ERAMASK);
            set => SetIntProperty(Command.ERAMASK, value);
        }
        public bool IsEraMaskModified => IsIntPropertyModifiedFromVanilla(Command.ERAMASK);
        public bool IsEraMaskSessionEdit => IsPropertyEditedInSession(Command.ERAMASK);

        /// <summary>
        /// Gets the era display string from the era bitmask.
        /// </summary>
        public string EraDisplay
        {
            get
            {
                var mask = EraMask ?? 0;
                if (mask == 0) return "None";
                var eras = new List<string>();
                if ((mask & 1) != 0) eras.Add("EA");
                if ((mask & 2) != 0) eras.Add("MA");
                if ((mask & 4) != 0) eras.Add("LA");
                return string.Join("/", eras);
            }
        }

        // ========================================
        // Unit References
        // ========================================

        public string CommanderDisplay
        {
            get
            {
                var result = _entity.TryGet<MonsterOrMontagRef>(Command.COM, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return GetMonsterOrMontagName(prop);
                }

                // VanillaModified fallback
                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<MonsterOrMontagRef>(Command.COM, out var vanillaProp);
                        if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                        {
                            return GetMonsterOrMontagName(vanillaProp);
                        }
                    }
                }
                return null;
            }
        }

        public string UnitDisplay
        {
            get
            {
                var result = _entity.TryGet<MonsterOrMontagRef>(Command.UNIT, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return GetMonsterOrMontagName(prop);
                }

                // VanillaModified fallback
                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<MonsterOrMontagRef>(Command.UNIT, out var vanillaProp);
                        if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                        {
                            return GetMonsterOrMontagName(vanillaProp);
                        }
                    }
                }
                return null;
            }
        }

        private string GetMonsterOrMontagName(MonsterOrMontagRef prop)
        {
            // Try to get the resolved entity
            if (prop.TryGetEntity(out var entity) && entity != null)
            {
                var name = entity.Name;
                if (!string.IsNullOrEmpty(name))
                    return $"{name} (#{entity.ID})";
                return $"#{entity.ID}";
            }

            // Fallback to export string if entity not resolved
            var exportStr = prop.ToExportString();
            if (!string.IsNullOrEmpty(exportStr))
            {
                // Check if it's a negative ID (montag)
                if (int.TryParse(exportStr, out var id) && id < 0)
                {
                    return $"Montag #{id}";
                }
                return exportStr;
            }
            return null;
        }

        public int? NrUnits
        {
            get => GetIntProperty(Command.NRUNITS);
            set => SetIntProperty(Command.NRUNITS, value);
        }
        public bool IsNrUnitsModified => IsIntPropertyModifiedFromVanilla(Command.NRUNITS);
        public bool IsNrUnitsSessionEdit => IsPropertyEditedInSession(Command.NRUNITS);

        public int? MinMen
        {
            get => GetIntProperty(Command.MINMEN);
            set => SetIntProperty(Command.MINMEN, value);
        }
        public bool IsMinMenModified => IsIntPropertyModifiedFromVanilla(Command.MINMEN);
        public bool IsMinMenSessionEdit => IsPropertyEditedInSession(Command.MINMEN);

        // ========================================
        // Economics
        // ========================================

        public int? MinPay
        {
            get => GetIntProperty(Command.MINPAY);
            set => SetIntProperty(Command.MINPAY, value);
        }
        public bool IsMinPayModified => IsIntPropertyModifiedFromVanilla(Command.MINPAY);
        public bool IsMinPaySessionEdit => IsPropertyEditedInSession(Command.MINPAY);

        public int? RecRate
        {
            get => GetIntProperty(Command.RECRATE);
            set => SetIntProperty(Command.RECRATE, value);
        }
        public bool IsRecRateModified => IsIntPropertyModifiedFromVanilla(Command.RECRATE);
        public bool IsRecRateSessionEdit => IsPropertyEditedInSession(Command.RECRATE);

        // ========================================
        // Equipment
        // ========================================

        public int? XP
        {
            get => GetIntProperty(Command.XP);
            set => SetIntProperty(Command.XP, value);
        }
        public bool IsXPModified => IsIntPropertyModifiedFromVanilla(Command.XP);
        public bool IsXPSessionEdit => IsPropertyEditedInSession(Command.XP);

        public int? RandEquip
        {
            get => GetIntProperty(Command.RANDEQUIP);
            set => SetIntProperty(Command.RANDEQUIP, value);
        }
        public bool IsRandEquipModified => IsIntPropertyModifiedFromVanilla(Command.RANDEQUIP);
        public bool IsRandEquipSessionEdit => IsPropertyEditedInSession(Command.RANDEQUIP);

        public string ItemDisplay
        {
            get
            {
                var result = _entity.TryGet<ItemRef>(Command.ITEM, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    return GetReferenceName(prop, EntityType.ITEM) ?? $"#{prop.ID}";
                }

                // VanillaModified fallback
                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<ItemRef>(Command.ITEM, out var vanillaProp);
                        if ((vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED) && vanillaProp != null)
                        {
                            return GetReferenceName(vanillaProp, EntityType.ITEM) ?? $"#{vanillaProp.ID}";
                        }
                    }
                }
                return null;
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
            var propertyName = GetPropertyNameForCommand(command);
            if (propertyName != null)
            {
                OnPropertyChanged(propertyName);
                OnPropertyChanged(propertyName + "Modified");
                OnPropertyChanged(propertyName + "SessionEdit");
            }

            // Refresh relevant badge collection
            RefreshIdentityBadges();
            RefreshUnitsBadges();
            RefreshEconomicsBadges();
            RefreshEquipmentBadges();
        }

        private string GetPropertyNameForCommand(Command command)
        {
            return command switch
            {
                Command.LEVEL => nameof(Level),
                Command.BOSSNAME => nameof(BossName),
                Command.ERAMASK => nameof(EraMask),
                Command.NRUNITS => nameof(NrUnits),
                Command.MINMEN => nameof(MinMen),
                Command.MINPAY => nameof(MinPay),
                Command.RECRATE => nameof(RecRate),
                Command.XP => nameof(XP),
                Command.RANDEQUIP => nameof(RandEquip),
                _ => null
            };
        }
    }
}
