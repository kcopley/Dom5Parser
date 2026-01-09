using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// ViewModel for Armor entities.
    /// </summary>
    public class ArmorViewModel : EntityViewModel
    {
        public ArmorViewModel(Armor entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Armor Armor => (Armor)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from armor_badges.json.
        /// </summary>
        protected override string EntityTypeName => "armor";

        // ========================================
        // Copy From Support (editable)
        // ========================================

        /// <summary>
        /// Gets or sets the CopyArmor reference ID.
        /// </summary>
        public int? CopyArmorId
        {
            get
            {
                var result = _entity.TryGet<ArmorRef>(Command.COPYARMOR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.ID;
                    return prop.ID;
                }
                return null;
            }
            set
            {
                if (value == null || value == 0)
                {
                    _entity.RemoveProperty(Command.COPYARMOR);
                }
                else
                {
                    _entity.Set<ArmorRef>(Command.COPYARMOR, p => p.Parse(Command.COPYARMOR, value.Value.ToString(), ""));
                    if (_entity.TryGet<ArmorRef>(Command.COPYARMOR, out var prop) == ReturnType.TRUE)
                        RecordPropertyChangeInSession(prop);
                }
                OnPropertyChanged(nameof(CopyArmorId));
                OnPropertyChanged(nameof(CopyArmorName));
                OnPropertyChanged(nameof(HasCopyArmor));

                // Refresh all properties that inherit from copyarmor
                RefreshAllCopyDependentProperties();
            }
        }

        /// <summary>
        /// Refreshes all properties and collections that depend on copyarmor inheritance.
        /// </summary>
        private void RefreshAllCopyDependentProperties()
        {
            RefreshStatsBadges();
            RefreshPropertyBadges();
        }

        /// <summary>
        /// Gets the CopyArmor reference name for display.
        /// </summary>
        public string CopyArmorName
        {
            get
            {
                var result = _entity.TryGet<ArmorRef>(Command.COPYARMOR, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"#{idEntity.ID}";
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        public bool HasCopyArmor => CopyArmorId.HasValue;

        // Cached reference items for copy armor selector
        private List<ReferenceItem> _availableArmorsForCopy;

        /// <summary>
        /// Gets the available armors as ReferenceItems for the copy armor selector.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableArmorsForCopy
        {
            get
            {
                if (_availableArmorsForCopy == null)
                {
                    _availableArmorsForCopy = CachedArmors
                        .Where(a => a.ID != ID) // Exclude self
                        .Select(a => new ReferenceItem { ID = a.ID, DisplayName = a.Name, Tag = a })
                        .ToList();
                }
                return _availableArmorsForCopy;
            }
        }

        // ========================================
        // Armor Type Display (derived from stats badge data)
        // ========================================

        /// <summary>
        /// Gets the armor type display name.
        /// Uses layered property access like the badge system.
        /// </summary>
        public string ArmorTypeDisplay
        {
            get
            {
                var armorType = GetIntProperty(Command.TYPE);
                return armorType switch
                {
                    1 => "Helmet",
                    4 => "Body Armor",
                    5 => "Shield",
                    6 => "Misc Body",
                    _ => armorType?.ToString() ?? "Unknown"
                };
            }
        }

        // ========================================
        // Stats Badge Collection (JSON-driven)
        // ========================================

        private ObservableCollection<PropertyItem> _statsBadges;
        private ObservableCollection<AvailablePropertyItem> _availableStatsBadges;

        public ObservableCollection<PropertyItem> StatsBadges
        {
            get { if (_statsBadges == null) RefreshStatsBadges(); return _statsBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailableStatsBadges
        {
            get { if (_availableStatsBadges == null) RefreshStatsBadges(); return _availableStatsBadges; }
        }

        // Commands for stats badge operations
        private RelayCommand<PropertyItem> _removeStatsBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addStatsBadgeCommand;

        public RelayCommand<PropertyItem> RemoveStatsBadgeCommand => _removeStatsBadgeCommand ??= CreateRemoveBadgeCommand(RefreshStatsBadges);
        public RelayCommand<AvailablePropertyItem> AddStatsBadgeCommand => _addStatsBadgeCommand ??= CreateAddBadgeCommand(RefreshStatsBadges);

        private void RefreshStatsBadges()
        {
            var (active, available) = BuildBadgesFromSection("stats", BadgeValueChangedHandler);
            _statsBadges = active;
            _availableStatsBadges = available;
            OnPropertyChanged(nameof(StatsBadges));
            OnPropertyChanged(nameof(AvailableStatsBadges));
            OnPropertyChanged(nameof(ArmorTypeDisplay)); // Update derived property
        }

        // ========================================
        // Properties Badge Collection (JSON-driven)
        // ========================================

        private ObservableCollection<PropertyItem> _propertyBadges;
        private ObservableCollection<AvailablePropertyItem> _availablePropertyBadges;

        public ObservableCollection<PropertyItem> PropertyBadges
        {
            get { if (_propertyBadges == null) RefreshPropertyBadges(); return _propertyBadges; }
        }
        public ObservableCollection<AvailablePropertyItem> AvailablePropertyBadges
        {
            get { if (_availablePropertyBadges == null) RefreshPropertyBadges(); return _availablePropertyBadges; }
        }

        // Commands for property badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler for all badge sections
        private EventHandler<int> _badgeValueChangedHandler;
        private EventHandler<int> BadgeValueChangedHandler => _badgeValueChangedHandler ??= CreateBadgeValueChangedHandler();

        private void RefreshPropertyBadges()
        {
            var (active, available) = BuildBadgesFromSection("properties", BadgeValueChangedHandler);
            _propertyBadges = active;
            _availablePropertyBadges = available;
            OnPropertyChanged(nameof(PropertyBadges));
            OnPropertyChanged(nameof(AvailablePropertyBadges));
        }

        protected override void OnPropertyRefreshedByHistory(Command command)
        {
            // Refresh the appropriate badge collection when undo/redo affects this entity
            RefreshStatsBadges();
            RefreshPropertyBadges();
        }
    }
}
