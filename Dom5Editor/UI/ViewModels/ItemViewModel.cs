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
    /// ViewModel for Item entities.
    /// Uses JSON-driven badge panels for most properties.
    /// Specialized sections remain for equipment display and path requirements.
    /// </summary>
    public class ItemViewModel : EntityViewModel
    {
        public ItemViewModel(Item entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Item Item => (Item)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from item_badges.json.
        /// </summary>
        protected override string EntityTypeName => "item";

        // ========================================
        // Copy From Support (editable)
        // ========================================

        /// <summary>
        /// Gets or sets the CopyItem reference ID.
        /// </summary>
        public int? CopyItemId
        {
            get
            {
                var result = _entity.TryGet<ItemRef>(Command.COPYITEM, out var prop, checkCopy: false);
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
                    _entity.RemoveProperty(Command.COPYITEM);
                }
                else
                {
                    _entity.Set<ItemRef>(Command.COPYITEM, p => p.Parse(Command.COPYITEM, value.Value.ToString(), ""));
                    if (_entity.TryGet<ItemRef>(Command.COPYITEM, out var prop) == ReturnType.TRUE)
                        RecordPropertyChangeInSession(prop);
                }
                OnPropertyChanged(nameof(CopyItemId));
                OnPropertyChanged(nameof(CopyItemName));
                OnPropertyChanged(nameof(HasCopyItem));

                // Refresh all properties that inherit from copyitem
                RefreshAllCopyDependentProperties();
            }
        }

        /// <summary>
        /// Refreshes all properties and collections that depend on copyitem inheritance.
        /// </summary>
        private void RefreshAllCopyDependentProperties()
        {
            RefreshPropertyBadges();
        }

        /// <summary>
        /// Gets the CopyItem reference name for display.
        /// </summary>
        public string CopyItemName
        {
            get
            {
                var result = _entity.TryGet<ItemRef>(Command.COPYITEM, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"#{idEntity.ID}";
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        public bool HasCopyItem => CopyItemId.HasValue;

        // Cached reference items for copy item selector
        private List<ReferenceItem> _availableItemsForCopy;

        /// <summary>
        /// Gets the available items as ReferenceItems for the copy item selector.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableItemsForCopy
        {
            get
            {
                if (_availableItemsForCopy == null)
                {
                    _availableItemsForCopy = CachedItems
                        .Where(i => i.ID != ID) // Exclude self
                        .Select(i => new ReferenceItem { ID = i.ID, DisplayName = i.Name, Tag = i })
                        .ToList();
                }
                return _availableItemsForCopy;
            }
        }

        // ========================================
        // Slot Type Display (for header and equipment logic)
        // ========================================

        /// <summary>
        /// Gets the item type value for slot logic.
        /// </summary>
        private int? ItemType
        {
            get
            {
                var result = _entity.TryGet<IntProperty>(Command.TYPE, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value;

                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<IntProperty>(Command.TYPE, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                            return vanillaProp?.Value;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the item slot type display name.
        /// Item types: 1=1-H Weapon, 2=2-H Weapon, 3=Missile Weapon, 4=Shield,
        /// 5=Body Armor, 6=Helmet, 7=Boots, 8=Misc, 9=Crown, 10=Barding
        /// </summary>
        public string ItemTypeDisplay
        {
            get
            {
                return ItemType switch
                {
                    1 => "1-H Weapon",
                    2 => "2-H Weapon",
                    3 => "Missile Weapon",
                    4 => "Shield",
                    5 => "Body Armor",
                    6 => "Helmet",
                    7 => "Boots",
                    8 => "Misc",
                    9 => "Crown",
                    10 => "Barding",
                    _ => ItemType?.ToString() ?? "Unknown"
                };
            }
        }

        /// <summary>
        /// Available slot types for ComboBox binding.
        /// Item types: 1=1-H Weapon, 2=2-H Weapon, 3=Missile Weapon, 4=Shield,
        /// 5=Body Armor, 6=Helmet, 7=Boots, 8=Misc, 9=Crown, 10=Barding
        /// </summary>
        public static List<SlotTypeOption> SlotTypes { get; } = new List<SlotTypeOption>
        {
            new SlotTypeOption(1, "1-H Weapon"),
            new SlotTypeOption(2, "2-H Weapon"),
            new SlotTypeOption(3, "Missile Weapon"),
            new SlotTypeOption(4, "Shield"),
            new SlotTypeOption(5, "Body Armor"),
            new SlotTypeOption(6, "Helmet"),
            new SlotTypeOption(7, "Boots"),
            new SlotTypeOption(8, "Misc"),
            new SlotTypeOption(9, "Crown"),
            new SlotTypeOption(10, "Barding"),
        };

        /// <summary>
        /// Gets or sets the selected slot type for ComboBox binding.
        /// </summary>
        public SlotTypeOption SelectedSlotType
        {
            get => SlotTypes.FirstOrDefault(s => s.Value == ItemType) ?? SlotTypes[7]; // Default to Misc (index 7, value 8)
            set
            {
                if (value != null)
                {
                    SetIntProperty(Command.TYPE, value.Value);
                    OnPropertyChanged(nameof(SelectedSlotType));
                    OnPropertyChanged(nameof(ItemTypeDisplay));
                    OnSlotTypeChanged(); // Refresh equipment options
                }
            }
        }

        // ========================================
        // Construction Requirements (PathSelector bindings)
        // ========================================

        public int? ConstLevel
        {
            get => GetIntProperty(Command.CONSTLEVEL);
            set => SetIntProperty(Command.CONSTLEVEL, value);
        }
        public bool IsConstLevelModified => IsIntPropertyModifiedFromVanilla(Command.CONSTLEVEL);
        public bool IsConstLevelSessionEdit => IsPropertyEditedInSession(Command.CONSTLEVEL);
        public bool IsConstLevelInherited => IsIntPropertyInherited(Command.CONSTLEVEL);

        public int? MainPath
        {
            get => GetIntProperty(Command.MAINPATH);
            set
            {
                SetIntProperty(Command.MAINPATH, value);
                RefreshPathDisplay();
            }
        }
        public bool IsMainPathModified => IsIntPropertyModifiedFromVanilla(Command.MAINPATH);
        public bool IsMainPathSessionEdit => IsPropertyEditedInSession(Command.MAINPATH);
        public bool IsMainPathInherited => IsIntPropertyInherited(Command.MAINPATH);

        public int? MainLevel
        {
            get => GetIntProperty(Command.MAINLEVEL);
            set
            {
                SetIntProperty(Command.MAINLEVEL, value);
                RefreshPathDisplay();
            }
        }
        public bool IsMainLevelModified => IsIntPropertyModifiedFromVanilla(Command.MAINLEVEL);
        public bool IsMainLevelSessionEdit => IsPropertyEditedInSession(Command.MAINLEVEL);
        public bool IsMainLevelInherited => IsIntPropertyInherited(Command.MAINLEVEL);

        public int? SecondaryPath
        {
            get => GetIntProperty(Command.SECONDARYPATH);
            set
            {
                SetIntProperty(Command.SECONDARYPATH, value);
                RefreshPathDisplay();
            }
        }
        public bool IsSecondaryPathModified => IsIntPropertyModifiedFromVanilla(Command.SECONDARYPATH);
        public bool IsSecondaryPathSessionEdit => IsPropertyEditedInSession(Command.SECONDARYPATH);
        public bool IsSecondaryPathInherited => IsIntPropertyInherited(Command.SECONDARYPATH);

        public int? SecondaryLevel
        {
            get => GetIntProperty(Command.SECONDARYLEVEL);
            set
            {
                SetIntProperty(Command.SECONDARYLEVEL, value);
                RefreshPathDisplay();
            }
        }
        public bool IsSecondaryLevelModified => IsIntPropertyModifiedFromVanilla(Command.SECONDARYLEVEL);
        public bool IsSecondaryLevelSessionEdit => IsPropertyEditedInSession(Command.SECONDARYLEVEL);
        public bool IsSecondaryLevelInherited => IsIntPropertyInherited(Command.SECONDARYLEVEL);

        /// <summary>
        /// Refreshes the gem cost display properties after path selection changes.
        /// Called by PathSelector change events in the view.
        /// </summary>
        public void RefreshPathDisplay()
        {
            OnPropertyChanged(nameof(PrimaryGemCost));
            OnPropertyChanged(nameof(SecondaryGemCost));
            OnPropertyChanged(nameof(HasGemCost));
            OnPropertyChanged(nameof(PrimaryPathLetter));
            OnPropertyChanged(nameof(SecondaryPathLetter));
        }

        // ========================================
        // Item Cost Modifiers (for gem cost calculations)
        // ========================================

        private int? ItemCost1
        {
            get
            {
                var result = _entity.TryGet<IntProperty>(Command.ITEMCOST1, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value;

                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<IntProperty>(Command.ITEMCOST1, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                            return vanillaProp?.Value;
                    }
                }
                return null;
            }
        }

        private int? ItemCost2
        {
            get
            {
                var result = _entity.TryGet<IntProperty>(Command.ITEMCOST2, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value;

                if (_source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    if (vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<IntProperty>(Command.ITEMCOST2, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                            return vanillaProp?.Value;
                    }
                }
                return null;
            }
        }

        // ========================================
        // Gem Cost Calculation
        // ========================================

        /// <summary>
        /// Calculates gem cost: 5 * level, reduced by itemcost percentage, rounded up.
        /// itemcost values are negative (e.g., -60 means 60% off).
        /// </summary>
        private int CalculateGemCost(int level, int? itemCost)
        {
            if (level <= 0) return 0;
            int baseCost = 5 * level;
            // itemcost is negative (e.g., -60 = 60% reduction), so we use absolute value
            int reductionPercent = Math.Abs(itemCost ?? 0);
            // Apply percentage reduction and round up
            double adjustedCost = baseCost * (100.0 - reductionPercent) / 100.0;
            return (int)Math.Ceiling(adjustedCost);
        }

        /// <summary>
        /// Gets the primary path gem cost.
        /// </summary>
        public int PrimaryGemCost => CalculateGemCost(MainLevel ?? 0, ItemCost1);

        /// <summary>
        /// Gets the secondary path gem cost.
        /// </summary>
        public int SecondaryGemCost => CalculateGemCost(SecondaryLevel ?? 0, ItemCost2);

        /// <summary>
        /// Whether this item has any gem cost to display.
        /// </summary>
        public bool HasGemCost => (MainPath >= 0 && (MainLevel ?? 0) > 0) ||
                                   (SecondaryPath >= 0 && (SecondaryLevel ?? 0) > 0);

        /// <summary>
        /// Gets the primary path letter for icon lookup (F, A, W, E, S, D, N, G, B).
        /// Returns null if no path selected or level is 0.
        /// </summary>
        public string PrimaryPathLetter =>
            (MainPath >= 0 && (MainLevel ?? 0) > 0) ? GetPathLetter(MainPath ?? -1) : null;

        /// <summary>
        /// Gets the secondary path letter for icon lookup.
        /// Returns null if no path selected or level is 0.
        /// </summary>
        public string SecondaryPathLetter =>
            (SecondaryPath >= 0 && (SecondaryLevel ?? 0) > 0) ? GetPathLetter(SecondaryPath ?? -1) : null;

        private static string GetPathLetter(int pathId)
        {
            return pathId switch
            {
                0 => "F",
                1 => "A",
                2 => "W",
                3 => "E",
                4 => "S",
                5 => "D",
                6 => "N",
                7 => "G",
                8 => "B",
                _ => null
            };
        }

        // ========================================
        // Equipment References (Weapon/Armor the item provides)
        // ========================================

        /// <summary>
        /// Gets the weapon reference display string.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public string WeaponDisplay
        {
            get
            {
                var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                if (prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if item provides a weapon.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public bool HasWeapon => HasReferenceProperty<WeaponRef>(Command.WEAPON);

        /// <summary>
        /// Gets the armor reference display string.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public string ArmorDisplay
        {
            get
            {
                var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                if (prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                    {
                        var name = idEntity.Name ?? idEntity.ID.ToString();
                        return $"{name} (#{idEntity.ID})";
                    }
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if item provides armor.
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        public bool HasArmor => HasReferenceProperty<ArmorRef>(Command.ARMOR);

        /// <summary>
        /// Returns true if item provides any equipment (weapon or armor).
        /// </summary>
        public bool HasEquipment => HasWeapon || HasArmor;

        // ========================================
        // Equipment Type Based on Slot
        // ========================================

        /// <summary>
        /// Returns true if this slot type uses weapon equipment.
        /// Weapon slots: 1-H Weapon (1), 2-H Weapon (2), Missile Weapon (3), Boots (7), Misc (8)
        /// </summary>
        public bool UsesWeaponEquipment => ItemType == 1 || ItemType == 2 || ItemType == 3 || ItemType == 7 || ItemType == 8;

        /// <summary>
        /// Returns true if this slot type uses armor equipment.
        /// Armor slots: Shield (4), Body Armor (5), Helmet (6), Crown (9), Barding (10)
        /// </summary>
        public bool UsesArmorEquipment => ItemType == 4 || ItemType == 5 || ItemType == 6 || ItemType == 9 || ItemType == 10;

        /// <summary>
        /// Gets the equipment type label based on slot type.
        /// </summary>
        public string EquipmentTypeLabel => UsesArmorEquipment ? "ARMOR" : "WEAPON";

        /// <summary>
        /// Gets the display string for the current equipment (weapon or armor).
        /// </summary>
        public string EquipmentDisplay => UsesArmorEquipment ? ArmorDisplay : WeaponDisplay;

        /// <summary>
        /// Gets the ID of the current equipment (weapon or armor).
        /// </summary>
        public int? EquipmentId
        {
            get
            {
                if (UsesArmorEquipment)
                {
                    var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                    return prop?.ID;
                }
                else
                {
                    var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                    return prop?.ID;
                }
            }
        }

        /// <summary>
        /// Available equipment items based on slot type.
        /// Uses centralized caches with filtering for weapon/armor based on slot.
        /// </summary>
        private List<AvailableEquipmentItem> _availableEquipment;
        public List<AvailableEquipmentItem> AvailableEquipment
        {
            get
            {
                if (_availableEquipment == null)
                    RefreshAvailableEquipment();
                return _availableEquipment;
            }
        }

        private void RefreshAvailableEquipment()
        {
            _availableEquipment = new List<AvailableEquipmentItem>();

            // Add "None" option first
            _availableEquipment.Add(new AvailableEquipmentItem { ID = 0, Name = "(None)", Source = "" });

            // Use cached lists from MainWindowViewModel for performance
            var sourceList = UsesArmorEquipment ? CachedArmors : CachedWeapons;
            _availableEquipment.AddRange(sourceList);

            OnPropertyChanged(nameof(AvailableEquipment));
        }

        /// <summary>
        /// Gets or sets the selected equipment from the dropdown.
        /// </summary>
        public AvailableEquipmentItem SelectedEquipment
        {
            get
            {
                var id = EquipmentId;
                if (id == null || id == 0)
                    return AvailableEquipment.FirstOrDefault(e => e.ID == 0);
                return AvailableEquipment.FirstOrDefault(e => e.ID == id) ?? AvailableEquipment.FirstOrDefault();
            }
            set
            {
                if (value == null) return;

                if (UsesArmorEquipment)
                {
                    ArmorId = value.ID == 0 ? "" : value.ID.ToString();
                }
                else
                {
                    WeaponId = value.ID == 0 ? "" : value.ID.ToString();
                }

                OnPropertyChanged(nameof(SelectedEquipment));
                OnPropertyChanged(nameof(EquipmentDisplay));
                OnPropertyChanged(nameof(EquipmentId));
            }
        }

        /// <summary>
        /// Called when slot type changes - clear incompatible equipment and refresh list.
        /// </summary>
        private void OnSlotTypeChanged()
        {
            // Clear incompatible equipment when slot type changes
            if (UsesArmorEquipment && HasWeapon)
            {
                // Switching to armor slot - clear weapon
                WeaponId = "";
            }
            else if (UsesWeaponEquipment && HasArmor)
            {
                // Switching to weapon slot - clear armor
                ArmorId = "";
            }

            _availableEquipment = null; // Force refresh
            OnPropertyChanged(nameof(UsesWeaponEquipment));
            OnPropertyChanged(nameof(UsesArmorEquipment));
            OnPropertyChanged(nameof(EquipmentTypeLabel));
            OnPropertyChanged(nameof(AvailableEquipment));
            OnPropertyChanged(nameof(SelectedEquipment));
            OnPropertyChanged(nameof(EquipmentDisplay));
            OnPropertyChanged(nameof(EquipmentId));
            OnPropertyChanged(nameof(HasWeapon));
            OnPropertyChanged(nameof(HasArmor));
            OnPropertyChanged(nameof(HasEquipment));
        }

        // ========================================
        // Equipment ID Setters (for changing references)
        // ========================================

        /// <summary>
        /// Gets or sets the weapon ID as a string for TextBox binding.
        /// Setting to empty/null removes the weapon reference.
        /// </summary>
        public string WeaponId
        {
            get
            {
                var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                return prop?.ID.ToString();
            }
            set
            {
                // Empty/null or "0" clears the weapon reference (#weapon 0 clears)
                if (string.IsNullOrWhiteSpace(value) || value == "0")
                {
                    // Remove the weapon reference
                    var result = _entity.TryGet<WeaponRef>(Command.WEAPON, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existingProp, "Remove weapon");
                        _history.Execute(cmd);
                    }
                }
                else if (int.TryParse(value, out int id) && id > 0)
                {
                    // Remove existing if present first
                    var result = _entity.TryGet<WeaponRef>(Command.WEAPON, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var removeCmd = new RemovePropertyCommand(_entity, existingProp, "Remove weapon");
                        _history.Execute(removeCmd);
                    }

                    // Create a new WeaponRef with the given ID (Parent must be set first)
                    var newRef = new WeaponRef { Parent = _entity, Command = Command.WEAPON };
                    newRef.ID = id;
                    newRef.Resolve();

                    if (_history != null)
                    {
                        var addCmd = new AddPropertyCommand(_entity, newRef, $"Set weapon #{id}");
                        _history.Execute(addCmd);
                    }
                }
                OnPropertyChanged(nameof(WeaponId));
                OnPropertyChanged(nameof(WeaponDisplay));
                OnPropertyChanged(nameof(HasWeapon));
                OnPropertyChanged(nameof(HasEquipment));
                OnPropertyChanged(nameof(WeaponDamage));
                OnPropertyChanged(nameof(WeaponAttack));
                OnPropertyChanged(nameof(WeaponDefense));
                OnPropertyChanged(nameof(WeaponLength));
                OnPropertyChanged(nameof(WeaponNrAtt));
            }
        }

        /// <summary>
        /// Gets or sets the armor ID as a string for TextBox binding.
        /// Setting to empty/null or 0 removes the armor reference.
        /// </summary>
        public string ArmorId
        {
            get
            {
                var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                return prop?.ID.ToString();
            }
            set
            {
                // Empty/null or "0" clears the armor reference (#armor 0 clears)
                if (string.IsNullOrWhiteSpace(value) || value == "0")
                {
                    // Remove the armor reference
                    var result = _entity.TryGet<ArmorRef>(Command.ARMOR, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existingProp, "Remove armor");
                        _history.Execute(cmd);
                    }
                }
                else if (int.TryParse(value, out int id) && id > 0)
                {
                    // Remove existing if present first
                    var result = _entity.TryGet<ArmorRef>(Command.ARMOR, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null && _history != null)
                    {
                        var removeCmd = new RemovePropertyCommand(_entity, existingProp, "Remove armor");
                        _history.Execute(removeCmd);
                    }

                    // Create a new ArmorRef with the given ID (Parent must be set first)
                    var newRef = new ArmorRef { Parent = _entity, Command = Command.ARMOR };
                    newRef.ID = id;
                    newRef.Resolve();

                    if (_history != null)
                    {
                        var addCmd = new AddPropertyCommand(_entity, newRef, $"Set armor #{id}");
                        _history.Execute(addCmd);
                    }
                }
                OnPropertyChanged(nameof(ArmorId));
                OnPropertyChanged(nameof(ArmorDisplay));
                OnPropertyChanged(nameof(HasArmor));
                OnPropertyChanged(nameof(HasEquipment));
                OnPropertyChanged(nameof(ArmorProtection));
                OnPropertyChanged(nameof(ArmorDefense));
                OnPropertyChanged(nameof(ArmorEncumbrance));
            }
        }

        // ========================================
        // Referenced Weapon Stats (Read-Only)
        // ========================================

        /// <summary>
        /// Gets the referenced weapon entity (if any).
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        private Weapon ReferencedWeapon
        {
            get
            {
                var prop = GetReferenceProperty<WeaponRef>(Command.WEAPON);
                if (prop?.Entity is Weapon weapon)
                    return weapon;
                return null;
            }
        }

        public int? WeaponDamage => GetWeaponIntProperty(Command.DMG);
        public int? WeaponAttack => GetWeaponIntProperty(Command.ATT);
        public int? WeaponDefense => GetWeaponIntProperty(Command.DEF);
        public int? WeaponLength => GetWeaponIntProperty(Command.LEN);
        public int? WeaponNrAtt => GetWeaponIntProperty(Command.NRATT);

        // Weapon Damage Types (flags)
        public bool WeaponIsPierce => GetWeaponFlag(Command.PIERCE);
        public bool WeaponIsSlash => GetWeaponFlag(Command.SLASH);
        public bool WeaponIsBlunt => GetWeaponFlag(Command.BLUNT);
        public bool WeaponIsFire => GetWeaponFlag(Command.FIRE);
        public bool WeaponIsCold => GetWeaponFlag(Command.COLD);
        public bool WeaponIsShock => GetWeaponFlag(Command.SHOCK);
        public bool WeaponIsPoison => GetWeaponFlag(Command.POISON);
        public bool WeaponIsAcid => GetWeaponFlag(Command.ACID);
        public bool WeaponIsMagic => GetWeaponFlag(Command.MAGIC);

        // Weapon Special Properties
        public bool WeaponIsArmorNegating => GetWeaponFlag(Command.ARMORNEGATING);
        public bool WeaponIsArmorPiercing => GetWeaponFlag(Command.ARMORPIERCING);
        public bool WeaponIsTwoHanded => GetWeaponFlag(Command.TWOHANDED);
        public bool WeaponIsFlail => GetWeaponFlag(Command.FLAIL);
        public bool WeaponIsNatural => GetWeaponFlag(Command.NATURAL);

        /// <summary>
        /// Gets a formatted string of weapon damage types.
        /// </summary>
        public string WeaponDamageTypes
        {
            get
            {
                var types = new List<string>();
                if (WeaponIsPierce) types.Add("Pierce");
                if (WeaponIsSlash) types.Add("Slash");
                if (WeaponIsBlunt) types.Add("Blunt");
                if (WeaponIsFire) types.Add("Fire");
                if (WeaponIsCold) types.Add("Cold");
                if (WeaponIsShock) types.Add("Shock");
                if (WeaponIsPoison) types.Add("Poison");
                if (WeaponIsAcid) types.Add("Acid");
                if (WeaponIsMagic) types.Add("Magic");
                return types.Count > 0 ? string.Join(", ", types) : null;
            }
        }

        /// <summary>
        /// Gets a formatted string of weapon special properties.
        /// </summary>
        public string WeaponSpecialProperties
        {
            get
            {
                var props = new List<string>();
                if (WeaponIsArmorNegating) props.Add("Armor Negating");
                if (WeaponIsArmorPiercing) props.Add("Armor Piercing");
                if (WeaponIsTwoHanded) props.Add("Two-Handed");
                if (WeaponIsFlail) props.Add("Flail");
                if (WeaponIsNatural) props.Add("Natural");
                return props.Count > 0 ? string.Join(", ", props) : null;
            }
        }

        // Secondary Effect
        public bool HasWeaponSecondaryEffect => GetWeaponSecondaryEffectId() != null;

        public string WeaponSecondaryEffectDisplay
        {
            get
            {
                var weapon = ReferencedWeapon;
                if (weapon == null) return null;

                // Check secondaryeffectalways first (100% chance)
                var alwaysResult = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
                if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp != null)
                {
                    var name = alwaysProp.Entity?.Name ?? $"#{alwaysProp.ID}";
                    return $"{name} (100%)";
                }

                // Check secondaryeffect (25% chance)
                var result = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                {
                    var name = prop.Entity?.Name ?? $"#{prop.ID}";
                    return $"{name} (25%)";
                }

                return null;
            }
        }

        private int? GetWeaponSecondaryEffectId()
        {
            var weapon = ReferencedWeapon;
            if (weapon == null) return null;

            var alwaysResult = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
            if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp != null)
                return alwaysProp.ID;

            var result = weapon.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
            if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                return prop.ID;

            return null;
        }

        private bool GetWeaponFlag(Command command)
        {
            var weapon = ReferencedWeapon;
            if (weapon == null) return false;

            // Flags are stored as CommandProperty (no value) or IntProperty with value
            var cmdResult = weapon.TryGet<CommandProperty>(command, out _);
            if (cmdResult == ReturnType.TRUE || cmdResult == ReturnType.COPIED)
                return true;

            var intResult = weapon.TryGet<IntProperty>(command, out var intProp);
            if ((intResult == ReturnType.TRUE || intResult == ReturnType.COPIED) && intProp?.Value != 0)
                return true;

            return false;
        }

        private int? GetWeaponIntProperty(Command command)
        {
            var weapon = ReferencedWeapon;
            if (weapon == null) return null;

            // Special handling for DMG which is stored as StringProperty
            if (command == Command.DMG)
            {
                var result = weapon.TryGet<StringProperty>(command, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                {
                    if (int.TryParse(prop.Value, out int val))
                        return val;
                }
                return null;
            }

            var intResult = weapon.TryGet<IntProperty>(command, out var intProp);
            if (intResult == ReturnType.TRUE || intResult == ReturnType.COPIED)
                return intProp?.Value;
            return null;
        }

        // ========================================
        // Referenced Armor Stats (Read-Only)
        // ========================================

        /// <summary>
        /// Gets the referenced armor entity (if any).
        /// Uses generic fallback for VanillaModified entities.
        /// </summary>
        private Armor ReferencedArmor
        {
            get
            {
                var prop = GetReferenceProperty<ArmorRef>(Command.ARMOR);
                if (prop?.Entity is Armor armor)
                    return armor;
                return null;
            }
        }

        public int? ArmorProtection => GetArmorIntProperty(Command.PROT);
        public int? ArmorDefense => GetArmorIntProperty(Command.DEF);
        public int? ArmorEncumbrance => GetArmorIntProperty(Command.ENC);

        private int? GetArmorIntProperty(Command command)
        {
            var armor = ReferencedArmor;
            if (armor == null) return null;

            var result = armor.TryGet<IntProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                return prop?.Value;
            return null;
        }

        // ========================================
        // Badge Collection (Unified)
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

        // Commands for badge operations
        private RelayCommand<PropertyItem> _removePropertyBadgeCommand;
        private RelayCommand<AvailablePropertyItem> _addPropertyBadgeCommand;

        public RelayCommand<PropertyItem> RemovePropertyBadgeCommand => _removePropertyBadgeCommand ??= CreateRemoveBadgeCommand(RefreshPropertyBadges);
        public RelayCommand<AvailablePropertyItem> AddPropertyBadgeCommand => _addPropertyBadgeCommand ??= CreateAddBadgeCommand(RefreshPropertyBadges);

        // Shared value changed handler
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
            // Refresh badge collection when undo/redo affects this entity
            RefreshPropertyBadges();

            // Refresh derived display properties that may have changed
            OnPropertyChanged(nameof(ItemTypeDisplay));
            RefreshPathDisplay();
            OnPropertyChanged(nameof(HasEquipment));
            OnPropertyChanged(nameof(WeaponDisplay));
            OnPropertyChanged(nameof(ArmorDisplay));
        }
    }
}
