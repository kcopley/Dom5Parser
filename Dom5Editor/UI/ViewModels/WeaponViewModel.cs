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
    /// ViewModel for Weapon entities.
    /// </summary>
    public class WeaponViewModel : EntityViewModel
    {
        public WeaponViewModel(Weapon entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
            : base(entity, history, source)
        {
        }

        public Weapon Weapon => (Weapon)_entity;

        /// <summary>
        /// Entity type name for loading badge configuration from weapon_badges.json.
        /// </summary>
        protected override string EntityTypeName => "weapon";

        // ========================================
        // Copy From Support (editable)
        // ========================================

        /// <summary>
        /// Gets or sets the CopyWeapon reference ID.
        /// </summary>
        public int? CopyWeaponId
        {
            get
            {
                var result = _entity.TryGet<WeaponRef>(Command.COPYWEAPON, out var prop, checkCopy: false);
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
                    _entity.RemoveProperty(Command.COPYWEAPON);
                }
                else
                {
                    _entity.Set<WeaponRef>(Command.COPYWEAPON, p => p.Parse(Command.COPYWEAPON, value.Value.ToString(), ""));
                    if (_entity.TryGet<WeaponRef>(Command.COPYWEAPON, out var prop) == ReturnType.TRUE)
                        RecordPropertyChangeInSession(prop);
                }
                OnPropertyChanged(nameof(CopyWeaponId));
                OnPropertyChanged(nameof(CopyWeaponName));
                OnPropertyChanged(nameof(HasCopyWeapon));

                // Refresh all properties that inherit from copyweapon
                RefreshAllCopyDependentProperties();
            }
        }

        /// <summary>
        /// Refreshes all properties and collections that depend on copyweapon inheritance.
        /// </summary>
        private void RefreshAllCopyDependentProperties()
        {
            RefreshStatsBadges();
            RefreshPropertyBadges();

            // Notify damage properties may have changed
            OnPropertyChanged(nameof(DamageRawValue));
            OnPropertyChanged(nameof(EffectiveDamageType));
            OnPropertyChanged(nameof(IsSummonWeapon));
            OnPropertyChanged(nameof(IsCloudWeapon));
            OnPropertyChanged(nameof(DamageLabel));
            OnPropertyChanged(nameof(Damage));
            OnPropertyChanged(nameof(DamageDisplayString));
            OnPropertyChanged(nameof(CanEditDamage));
        }

        /// <summary>
        /// Gets the CopyWeapon reference name for display.
        /// </summary>
        public string CopyWeaponName
        {
            get
            {
                var result = _entity.TryGet<WeaponRef>(Command.COPYWEAPON, out var prop, checkCopy: false);
                if (result == ReturnType.TRUE && prop != null)
                {
                    if (prop.Entity != null && prop.Entity is IDEntity idEntity)
                        return idEntity.Name ?? $"#{idEntity.ID}";
                    return prop.Name ?? $"#{prop.ID}";
                }
                return null;
            }
        }

        public bool HasCopyWeapon => CopyWeaponId.HasValue;

        // Cached reference items for copy weapon selector
        private List<ReferenceItem> _availableWeaponsForCopy;

        /// <summary>
        /// Gets the available weapons as ReferenceItems for the copy weapon selector.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableWeaponsForCopy
        {
            get
            {
                if (_availableWeaponsForCopy == null)
                {
                    _availableWeaponsForCopy = CachedWeapons
                        .Where(w => w.ID != ID) // Exclude self
                        .Select(w => new ReferenceItem { ID = w.ID, DisplayName = w.Name, Tag = w })
                        .ToList();
                }
                return _availableWeaponsForCopy;
            }
        }

        // ========================================
        // Damage - Special Handling for Summon/Cloud Weapons
        // ========================================

        // Special damage type constants
        private const string DamageTypeSummon = "summonunits";
        private const string DamageTypeCloud = "cloud";
        private const int DefaultSummonMonsterId = 297;

        /// <summary>
        /// Gets the raw damage string value (could be a number, "summonunits", "cloud", etc.)
        /// </summary>
        public string DamageRawValue
        {
            get
            {
                var result = _entity.TryGet<StringProperty>(Command.DMG, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                    return prop.Value;
                return null;
            }
        }

        /// <summary>
        /// Gets the effective damage type by checking current value and inherited value.
        /// Returns "summonunits", "cloud", or null for normal/numeric damage.
        /// </summary>
        public string EffectiveDamageType
        {
            get
            {
                // First check direct value
                var rawValue = DamageRawValue;
                if (rawValue == DamageTypeSummon || rawValue == DamageTypeCloud)
                    return rawValue;

                // Check if inherited from copyweapon source
                var copyResult = _entity.TryGet<WeaponRef>(Command.COPYWEAPON, out var copyRef);
                if (copyResult == ReturnType.TRUE && copyRef?.Entity is Weapon sourceWeapon)
                {
                    var sourceResult = sourceWeapon.TryGet<StringProperty>(Command.DMG, out var sourceProp);
                    if ((sourceResult == ReturnType.TRUE || sourceResult == ReturnType.COPIED) && sourceProp != null)
                    {
                        if (sourceProp.Value == DamageTypeSummon || sourceProp.Value == DamageTypeCloud)
                            return sourceProp.Value;
                    }
                }

                return null; // Normal numeric damage
            }
        }

        /// <summary>
        /// Returns true if this weapon summons units (damage value is monster ID).
        /// </summary>
        public bool IsSummonWeapon => EffectiveDamageType == DamageTypeSummon;

        /// <summary>
        /// Returns true if this weapon creates a cloud (damage is read-only).
        /// </summary>
        public bool IsCloudWeapon => EffectiveDamageType == DamageTypeCloud;

        /// <summary>
        /// Gets the label for the damage field based on weapon type.
        /// </summary>
        public string DamageLabel => IsSummonWeapon ? "Summon ID" : (IsCloudWeapon ? "Effect" : "DMG");

        /// <summary>
        /// Gets or sets weapon damage value. For summon weapons, this is the monster ID.
        /// For cloud weapons, this is read-only. For normal weapons, this is damage.
        /// </summary>
        public int? Damage
        {
            get
            {
                var rawValue = DamageRawValue;
                if (rawValue == null)
                    return null;

                // If it's a special type string, return the default or null
                if (rawValue == DamageTypeSummon)
                    return DefaultSummonMonsterId;
                if (rawValue == DamageTypeCloud)
                    return null; // Cloud has no numeric value

                // Try to parse as integer
                if (int.TryParse(rawValue, out int val))
                    return val;

                return null;
            }
            set
            {
                // Don't allow editing cloud weapons
                if (IsCloudWeapon && EffectiveDamageType == DamageTypeCloud)
                    return;

                if (value.HasValue)
                {
                    var result = _entity.TryGet<StringProperty>(Command.DMG, out var prop, checkCopy: false);
                    if (result == ReturnType.TRUE && prop != null)
                    {
                        prop.Value = value.Value.ToString();
                    }
                    else
                    {
                        // Create new property
                        SetIntProperty(Command.DMG, value);
                    }
                }
                else
                {
                    _entity.RemoveProperty(Command.DMG);
                }
                OnPropertyChanged(nameof(Damage));
                OnPropertyChanged(nameof(DamageRawValue));
                OnPropertyChanged(nameof(IsDamageModified));
            }
        }

        /// <summary>
        /// Gets a display string for damage that includes context for special types.
        /// </summary>
        public string DamageDisplayString
        {
            get
            {
                if (IsCloudWeapon)
                    return "Cloud";
                if (IsSummonWeapon)
                {
                    var monsterId = Damage ?? DefaultSummonMonsterId;
                    // Try to get monster name
                    if (VanillaLoader.Vanilla?.Database.TryGetValue(EntityType.MONSTER, out var monsterSet) == true &&
                        monsterSet.TryGetValue(monsterId, out var monster))
                    {
                        return $"Summons: {monster.Name ?? $"#{monsterId}"}";
                    }
                    return $"Summons: #{monsterId}";
                }
                return Damage?.ToString() ?? "";
            }
        }

        public bool IsDamageModified
        {
            get
            {
                var currentResult = _entity.TryGet<StringProperty>(Command.DMG, out var currentProp, checkCopy: false);
                var hasCurrent = currentResult == ReturnType.TRUE;

                if (_entity is IDEntity idEntity &&
                    VanillaLoader.Vanilla?.Database.TryGetValue(EntityType.WEAPON, out var vanillaSet) == true)
                {
                    if (vanillaSet.TryGetValue(idEntity.ID, out var vanillaEntity) && vanillaEntity != null)
                    {
                        var vanillaResult = vanillaEntity.TryGet<StringProperty>(Command.DMG, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            return hasCurrent && currentProp?.Value != vanillaProp?.Value;
                        }
                    }
                }
                return hasCurrent;
            }
        }
        public bool IsDamageSessionEdit => IsPropertyEditedInSession(Command.DMG);
        public bool IsDamageInherited
        {
            get
            {
                var result = _entity.TryGet<StringProperty>(Command.DMG, out _, checkCopy: false);
                return result == ReturnType.COPIED;
            }
        }

        /// <summary>
        /// Returns true if damage can be edited (not a cloud weapon with no override).
        /// </summary>
        public bool CanEditDamage => !IsCloudWeapon;

        // ========================================
        // DynamicPropertyEditor Support for Damage
        // ========================================

        /// <summary>
        /// Gets the editor mode for the damage property.
        /// Returns "ref" for summon weapons (monster selector), "readonly" for cloud, "int" for normal.
        /// </summary>
        public string DamageEditorMode
        {
            get
            {
                if (IsCloudWeapon) return "readonly";
                if (IsSummonWeapon) return "ref";
                return "int";
            }
        }

        /// <summary>
        /// Gets or sets the monster ID for summon weapons.
        /// Used in ref mode for the monster selector.
        /// </summary>
        public int? DamageMonsterId
        {
            get => Damage;
            set
            {
                if (IsSummonWeapon)
                {
                    Damage = value;
                    OnPropertyChanged(nameof(DamageMonsterId));
                    OnPropertyChanged(nameof(DamageSecondaryText));
                }
            }
        }

        /// <summary>
        /// Gets the secondary display text for the damage field.
        /// Shows the summoned monster name for summon weapons.
        /// </summary>
        public string DamageSecondaryText
        {
            get
            {
                if (IsSummonWeapon)
                {
                    return DamageDisplayString; // "Summons: MonsterName"
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the readonly display text for cloud weapons.
        /// </summary>
        public string DamageReadOnlyText => IsCloudWeapon ? "Read-only cloud effect" : "";

        // Cached reference items for monster selector
        private List<ReferenceItem> _availableMonstersForDamage;

        /// <summary>
        /// Gets the available monsters as ReferenceItems for the damage monster selector.
        /// </summary>
        public IEnumerable<ReferenceItem> AvailableMonstersForDamage
        {
            get
            {
                if (_availableMonstersForDamage == null)
                {
                    _availableMonstersForDamage = CachedMonsters
                        .Select(m => new ReferenceItem { ID = m.ID, DisplayName = m.Name, Tag = m })
                        .ToList();
                }
                return _availableMonstersForDamage;
            }
        }

        // ========================================
        // Derived Display Properties (used in header/summary)
        // ========================================

        /// <summary>
        /// Gets the damage type string for display (Pierce, Slash, Blunt, Fire, etc.).
        /// Used in the Stats summary section.
        /// </summary>
        public string DamageTypes
        {
            get
            {
                var types = new List<string>();
                if (GetWeaponFlag(Command.PIERCE)) types.Add("Pierce");
                if (GetWeaponFlag(Command.SLASH)) types.Add("Slash");
                if (GetWeaponFlag(Command.BLUNT)) types.Add("Blunt");
                if (GetWeaponFlag(Command.FIRE)) types.Add("Fire");
                if (GetWeaponFlag(Command.COLD)) types.Add("Cold");
                if (GetWeaponFlag(Command.SHOCK)) types.Add("Shock");
                if (GetWeaponFlag(Command.POISON)) types.Add("Poison");
                if (GetWeaponFlag(Command.ACID)) types.Add("Acid");
                if (GetWeaponFlag(Command.MAGIC)) types.Add("Magic");
                if (GetWeaponFlag(Command.HOLY)) types.Add("Holy");
                if (GetWeaponFlag(Command.DEMON)) types.Add("Demon");
                return types.Count > 0 ? string.Join(", ", types) : null;
            }
        }

        /// <summary>
        /// Gets special properties string for display (Armor Negating, Armor Piercing, etc.).
        /// Used in the Stats summary section.
        /// </summary>
        public string SpecialProperties
        {
            get
            {
                var props = new List<string>();
                if (GetWeaponFlag(Command.ARMORNEGATING)) props.Add("Armor Negating");
                if (GetWeaponFlag(Command.ARMORPIERCING)) props.Add("Armor Piercing");
                if (GetWeaponFlag(Command.HARDMRNEG)) props.Add("Hard MR Negating");
                if (GetWeaponFlag(Command.MRNEGATES)) props.Add("MR Negates");
                if (GetWeaponFlag(Command.MIND)) props.Add("Mind");
                if (GetWeaponFlag(Command.UNDEADIMMUNE)) props.Add("Undead Immune");
                if (GetWeaponFlag(Command.INANIMATEIMMUNE)) props.Add("Inanimate Immune");
                if (GetWeaponFlag(Command.FLYSPR)) props.Add("Flying Projectile");
                if (GetWeaponFlag(Command.TWOHANDED)) props.Add("Two-Handed");
                if (GetWeaponFlag(Command.NATURAL)) props.Add("Natural");
                if (GetWeaponFlag(Command.CHARGE)) props.Add("Charge");
                if (GetWeaponFlag(Command.FLAIL)) props.Add("Flail");
                if (GetWeaponFlag(Command.BONUS)) props.Add("Bonus");
                if (GetWeaponFlag(Command.NOSTR)) props.Add("No Strength");
                return props.Count > 0 ? string.Join(", ", props) : null;
            }
        }

        private bool GetWeaponFlag(Command command)
        {
            var result = _entity.TryGet<IntProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                return prop != null && prop.Value != 0;

            // Check vanilla for VanillaModified entities
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
                    return (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                           && vanillaProp != null && vanillaProp.Value != 0;
                }
            }

            return false;
        }

        // ========================================
        // Secondary Effect Support (derived display)
        // ========================================

        /// <summary>
        /// Returns true if this weapon has a secondary effect (either 25% or 100% chance).
        /// </summary>
        public bool HasSecondaryEffect
        {
            get
            {
                var alwaysResult = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out _);
                if (alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED)
                    return true;

                var result = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out _);
                return result == ReturnType.TRUE || result == ReturnType.COPIED;
            }
        }

        /// <summary>
        /// Gets the display string for the secondary effect, showing name and chance.
        /// </summary>
        public string SecondaryEffectDisplay
        {
            get
            {
                // Check for secondaryeffectalways first (100% chance)
                var alwaysResult = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
                if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp != null)
                {
                    var name = alwaysProp.Entity?.Name ?? $"#{alwaysProp.ID}";
                    return $"{name} (100%)";
                }

                // Check for secondaryeffect (25% chance)
                var result = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop != null)
                {
                    var name = prop.Entity?.Name ?? $"#{prop.ID}";
                    return $"{name} (25%)";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the referenced secondary effect weapon entity, if any.
        /// </summary>
        public Weapon SecondaryEffectWeapon
        {
            get
            {
                var alwaysResult = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECTALWAYS, out var alwaysProp);
                if ((alwaysResult == ReturnType.TRUE || alwaysResult == ReturnType.COPIED) && alwaysProp?.Entity is Weapon w1)
                    return w1;

                var result = _entity.TryGet<WeaponRef>(Command.SECONDARYEFFECT, out var prop);
                if ((result == ReturnType.TRUE || result == ReturnType.COPIED) && prop?.Entity is Weapon w2)
                    return w2;

                return null;
            }
        }

        // ========================================
        // Stats Badge Collection (JSON-driven, excludes DMG)
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
            // Handle damage specially
            if (command == Command.DMG || command == Command.DAMAGE)
            {
                OnPropertyChanged(nameof(Damage));
                OnPropertyChanged(nameof(DamageRawValue));
                OnPropertyChanged(nameof(DamageDisplayString));
                OnPropertyChanged(nameof(IsDamageModified));
                OnPropertyChanged(nameof(IsDamageSessionEdit));
                OnPropertyChanged(nameof(IsDamageInherited));
                OnPropertyChanged(nameof(IsSummonWeapon));
                OnPropertyChanged(nameof(IsCloudWeapon));
                OnPropertyChanged(nameof(DamageLabel));
                OnPropertyChanged(nameof(CanEditDamage));
                // DynamicPropertyEditor support
                OnPropertyChanged(nameof(DamageEditorMode));
                OnPropertyChanged(nameof(DamageMonsterId));
                OnPropertyChanged(nameof(DamageSecondaryText));
                OnPropertyChanged(nameof(DamageReadOnlyText));
            }

            // Refresh badge collections
            RefreshStatsBadges();
            RefreshPropertyBadges();

            // Refresh derived display properties that may have changed
            OnPropertyChanged(nameof(DamageTypes));
            OnPropertyChanged(nameof(SpecialProperties));
            OnPropertyChanged(nameof(HasSecondaryEffect));
            OnPropertyChanged(nameof(SecondaryEffectDisplay));
        }
    }
}
