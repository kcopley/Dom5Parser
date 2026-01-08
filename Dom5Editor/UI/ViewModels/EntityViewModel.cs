using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;
using Dom5Editor.UI;

namespace Dom5Editor.UI.Views
{
    /// <summary>
    /// Indicates the source/state of an entity.
    /// </summary>
    public enum EntitySource
    {
        /// <summary>Entity from vanilla (base game) with no modifications.</summary>
        Vanilla,
        /// <summary>Vanilla entity that has been modified by the loaded mod.</summary>
        VanillaModified,
        /// <summary>Entity created by the loaded mod (not in vanilla).</summary>
        FromMod,
        /// <summary>Entity created in the current editing session.</summary>
        New
    }

    /// <summary>
    /// Base ViewModel for all entity types.
    /// Provides common properties and functionality.
    /// </summary>
    public abstract class EntityViewModel : INotifyPropertyChanged
    {
        protected readonly IDEntity _entity;
        protected readonly CommandHistory _history;
        protected EntitySource _source;
        private bool _hasSessionChanges;

        /// <summary>
        /// Cached reference to the vanilla version of this entity (null if entity is new or not in vanilla).
        /// </summary>
        private IDEntity _vanillaEntity;
        private bool _vanillaEntityResolved;

        /// <summary>
        /// Reference to the ChangesMod for tracking session edits.
        /// </summary>
        private ChangesMod _changesMod;

        public event PropertyChangedEventHandler PropertyChanged;

        protected EntityViewModel(IDEntity entity, CommandHistory history, EntitySource source = EntitySource.Vanilla)
        {
            _entity = entity;
            _history = history;
            _source = source;

            // Subscribe to undo/redo notifications to refresh the view
            if (_history != null)
            {
                _history.PropertyChangedByHistory += OnPropertyChangedByHistory;
            }
        }

        /// <summary>
        /// Handles property change notifications from undo/redo operations.
        /// Refreshes the view when this entity's properties are affected.
        /// </summary>
        private void OnPropertyChangedByHistory(object sender, PropertyChangedByHistoryEventArgs e)
        {
            // Only handle changes to this entity
            if (e.Entity != _entity)
                return;

            // Notify that the property may have changed
            // We use a generic approach - notify common property name patterns
            var commandName = e.PropertyCommand.ToString();
            OnPropertyChanged(commandName);

            // Also notify related properties that might be derived from this
            OnPropertyChanged($"Is{commandName}Modified");
            OnPropertyChanged($"Is{commandName}SessionEdit");
            OnPropertyChanged($"Is{commandName}Inherited");

            // Notify that session changes flag may have changed
            OnPropertyChanged(nameof(HasSessionChanges));

            // Call virtual method for subclass-specific refresh
            OnPropertyRefreshedByHistory(e.PropertyCommand);
        }

        /// <summary>
        /// Called when a property is refreshed via undo/redo.
        /// Override in subclasses to handle specific property refresh needs.
        /// </summary>
        protected virtual void OnPropertyRefreshedByHistory(Command command)
        {
            // Default: no-op. Subclasses can override.
        }

        /// <summary>
        /// The underlying entity.
        /// </summary>
        public IDEntity Entity => _entity;

        // ========================================
        // Entity Navigation
        // ========================================

        /// <summary>
        /// Static reference to MainWindowViewModel for navigation.
        /// Must be set during application startup.
        /// </summary>
        private static MainWindowViewModel _mainViewModel;

        /// <summary>
        /// Sets the MainWindowViewModel reference for navigation.
        /// Call this during application initialization.
        /// </summary>
        public static void SetMainViewModel(MainWindowViewModel vm)
        {
            _mainViewModel = vm;
        }

        /// <summary>
        /// Gets the MainWindowViewModel for navigation.
        /// </summary>
        public static MainWindowViewModel MainViewModel => _mainViewModel;

        // ========================================
        // Cached Entity Lists (from MainWindowViewModel)
        // ========================================

        /// <summary>
        /// Gets the cached list of all available weapons for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedWeapons =>
            _mainViewModel?.CachedWeapons ?? Array.Empty<AvailableEquipmentItem>();

        /// <summary>
        /// Gets the cached list of all available armor for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedArmors =>
            _mainViewModel?.CachedArmors ?? Array.Empty<AvailableEquipmentItem>();

        /// <summary>
        /// Gets the cached list of all available monsters for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedMonsters =>
            _mainViewModel?.CachedMonsters ?? Array.Empty<AvailableEquipmentItem>();

        /// <summary>
        /// Gets the cached list of all available items for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedItems =>
            _mainViewModel?.CachedItems ?? Array.Empty<AvailableEquipmentItem>();

        /// <summary>
        /// Gets the cached list of all available spells for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedSpells =>
            _mainViewModel?.CachedSpells ?? Array.Empty<AvailableEquipmentItem>();

        /// <summary>
        /// Gets the cached list of all available sites for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedSites =>
            _mainViewModel?.CachedSites ?? Array.Empty<AvailableEquipmentItem>();

        /// <summary>
        /// Gets the cached list of all available nations for dropdowns.
        /// </summary>
        protected static IReadOnlyList<AvailableEquipmentItem> CachedNations =>
            _mainViewModel?.CachedNations ?? Array.Empty<AvailableEquipmentItem>();

        // Cached ReferenceItem lists (lazily converted from AvailableEquipmentItem)
        private static readonly Dictionary<string, IReadOnlyList<ReferenceItem>> _referenceItemCaches = new();

        /// <summary>
        /// Gets available ReferenceItems for a given reference type (monster, weapon, etc.).
        /// Used for searchable dropdowns in editable reference badges.
        /// </summary>
        protected static IEnumerable<ReferenceItem> GetAvailableReferencesForType(string refType)
        {
            if (string.IsNullOrEmpty(refType))
                return null;

            var key = refType.ToLowerInvariant();
            if (_referenceItemCaches.TryGetValue(key, out var cached))
                return cached;

            var source = GetCachedListForType(key);
            if (source == null)
                return null;

            var converted = ConvertToReferenceItems(source);
            _referenceItemCaches[key] = converted;
            return converted;
        }

        /// <summary>
        /// Gets the cached AvailableEquipmentItem list for a reference type.
        /// </summary>
        private static IReadOnlyList<AvailableEquipmentItem> GetCachedListForType(string refType)
        {
            return refType switch
            {
                "monster" => CachedMonsters,
                "weapon" => CachedWeapons,
                "armor" => CachedArmors,
                "item" => CachedItems,
                "spell" => CachedSpells,
                "site" => CachedSites,
                "nation" => CachedNations,
                _ => null
            };
        }

        /// <summary>
        /// Converts AvailableEquipmentItem list to ReferenceItem list.
        /// </summary>
        private static IReadOnlyList<ReferenceItem> ConvertToReferenceItems(IReadOnlyList<AvailableEquipmentItem> items)
        {
            if (items == null || items.Count == 0)
                return Array.Empty<ReferenceItem>();

            var result = new List<ReferenceItem>(items.Count);
            foreach (var item in items)
            {
                result.Add(new ReferenceItem
                {
                    ID = item.ID,
                    DisplayName = item.Name,
                    Tag = item
                });
            }
            return result;
        }

        /// <summary>
        /// Clears the cached ReferenceItem lists. Call when the mod changes.
        /// </summary>
        public static void ClearReferenceItemCaches()
        {
            _referenceItemCaches.Clear();
        }

        /// <summary>
        /// Generic navigation command that works for any reference type.
        /// Accepts PropertyItem (for badge clicks) or ValueTuple&lt;string, int&gt; (refType, id).
        /// </summary>
        public ICommand NavigateToReferenceCommand => new RelayCommand<object>(param =>
        {
            if (_mainViewModel == null)
                return;

            if (param is PropertyItem badge && badge.IsReference)
            {
                // Navigation from badge click
                var entityType = BadgeConfigLoader.GetEntityTypeFromRefType(badge.ReferenceType);
                if (entityType.HasValue)
                    _mainViewModel.NavigateToEntity(entityType.Value, badge.ReferenceId);
            }
            else if (param is ValueTuple<string, int> tuple)
            {
                // Navigation from equipment/reference click with (refType, id)
                var entityType = BadgeConfigLoader.GetEntityTypeFromRefType(tuple.Item1);
                if (entityType.HasValue)
                    _mainViewModel.NavigateToEntity(entityType.Value, tuple.Item2);
            }
        });

        // ========================================
        // Badge Configuration Infrastructure
        // ========================================

        /// <summary>
        /// Gets the entity type name for loading badge configuration.
        /// Override in derived classes to return "monster", "weapon", "armor", etc.
        /// Returns null if the entity type doesn't support badge configuration.
        /// </summary>
        protected virtual string EntityTypeName => null;

        /// <summary>
        /// Cache for badge configurations per entity type.
        /// </summary>
        private static readonly Dictionary<string, BadgeConfig> _badgeConfigCache = new();

        /// <summary>
        /// Gets the badge configuration for this entity type.
        /// Returns null if EntityTypeName is null or config doesn't exist.
        /// </summary>
        protected BadgeConfig GetBadgeConfig()
        {
            var typeName = EntityTypeName;
            if (string.IsNullOrEmpty(typeName))
                return null;

            if (!_badgeConfigCache.TryGetValue(typeName, out var config))
            {
                config = BadgeConfigLoader.LoadConfig(typeName);
                if (config != null)
                {
                    _badgeConfigCache[typeName] = config;
                }
            }
            return config;
        }

        /// <summary>
        /// Clears the badge configuration cache (useful for reloading during development).
        /// </summary>
        public static void ClearBadgeConfigCache()
        {
            _badgeConfigCache.Clear();
            BadgeConfigLoader.ClearCache();
        }

        /// <summary>
        /// Builds badges for a section using JSON configuration.
        /// Uses layered property access: vanilla -> mod -> session changes.
        /// This is the core method for populating badge collections from JSON config.
        /// </summary>
        /// <param name="sectionId">The section ID from the JSON config (e.g., "types", "general", "combat")</param>
        /// <param name="valueChangedHandler">Optional handler called when a badge value changes</param>
        /// <returns>Tuple of (active badges, available badges for adding)</returns>
        protected (ObservableCollection<PropertyItem> active, ObservableCollection<AvailablePropertyItem> available)
            BuildBadgesFromSection(string sectionId, EventHandler<int> valueChangedHandler = null)
        {
            var active = new ObservableCollection<PropertyItem>();
            var available = new ObservableCollection<AvailablePropertyItem>();
            var usedCommands = new HashSet<Command>();
            // Track ref commands that have at least one instance (for "available" dropdown logic)
            var refCommandsWithValues = new HashSet<Command>();

            var badgeConfig = GetBadgeConfig();
            if (badgeConfig == null)
                return (active, available);

            var section = badgeConfig.GetSection(sectionId);
            if (section == null)
                return (active, available);

            // Get vanilla entity for layered comparison
            var vanillaEntity = GetVanillaEntity();

            foreach (var cmdDef in section.Commands)
            {
                if (!BadgeConfigLoader.TryGetCommand(cmdDef, out var command))
                    continue;

                // Handle reference properties (multi-value, needs special layered logic)
                if (cmdDef.IsRef)
                {
                    var refBadges = BuildReferenceBadges(cmdDef, command, section.ReadOnly, vanillaEntity);
                    foreach (var badge in refBadges)
                    {
                        active.Add(badge);
                    }
                    if (refBadges.Count > 0)
                    {
                        refCommandsWithValues.Add(command);
                    }
                    // Reference commands can have multiple values, so they're always "available" for adding more
                    // (don't add to usedCommands to exclude from available list)
                    continue;
                }

                // Layer 1: Get vanilla value (base layer, includes vanilla copystats)
                bool vanillaHasValue = false;
                bool vanillaIsCopied = false;
                int? vanillaValue = null;
                bool vanillaFlagValue = false;
                // For IntIntProperty
                int? vanillaValue1 = null;
                int? vanillaValue2 = null;
                // For StringProperty
                string vanillaStringValue = null;
                // For BitmaskProperty
                ulong? vanillaBitmaskValue = null;

                if (vanillaEntity != null)
                {
                    if (cmdDef.IsFlag)
                    {
                        var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);
                        vanillaFlagValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;
                        vanillaHasValue = vanillaFlagValue;
                        vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                    }
                    else if (cmdDef.IsInt)
                    {
                        var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            vanillaValue = vanillaProp?.Value;
                            vanillaHasValue = true;
                            vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                        }
                    }
                    else if (cmdDef.IsIntInt)
                    {
                        var vanillaResult = vanillaEntity.TryGet<IntIntProperty>(command, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            vanillaValue1 = vanillaProp?.Value1;
                            vanillaValue2 = vanillaProp?.Value2;
                            vanillaHasValue = true;
                            vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                        }
                    }
                    else if (cmdDef.IsString)
                    {
                        var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            vanillaStringValue = vanillaProp?.Value;
                            vanillaHasValue = true;
                            vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                        }
                    }
                    else if (cmdDef.IsBitmask)
                    {
                        var vanillaResult = vanillaEntity.TryGet<BitmaskProperty>(command, out var vanillaProp);
                        if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                        {
                            vanillaBitmaskValue = vanillaProp?.Value;
                            vanillaHasValue = true;
                            vanillaIsCopied = vanillaResult == ReturnType.COPIED;
                        }
                    }
                }

                // Layer 2: Get current entity value (mod + session, includes mod copystats)
                bool entityHasValue = false;
                bool entityIsCopied = false;
                bool entityHasDirect = false; // Has the property directly (not from copystats)
                int? entityValue = null;
                bool entityFlagValue = false;
                // For IntIntProperty
                int? entityValue1 = null;
                int? entityValue2 = null;
                // For StringProperty
                string entityStringValue = null;
                // For BitmaskProperty
                ulong? entityBitmaskValue = null;

                if (cmdDef.IsFlag)
                {
                    var entityResult = _entity.TryGet<CommandProperty>(command, out _);
                    entityFlagValue = entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED;
                    entityHasValue = entityFlagValue;
                    entityIsCopied = entityResult == ReturnType.COPIED;
                    entityHasDirect = entityResult == ReturnType.TRUE;
                }
                else if (cmdDef.IsInt)
                {
                    var entityResult = _entity.TryGet<IntProperty>(command, out var entityProp);
                    if (entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED)
                    {
                        entityValue = entityProp?.Value;
                        entityHasValue = true;
                        entityIsCopied = entityResult == ReturnType.COPIED;
                        entityHasDirect = entityResult == ReturnType.TRUE;
                    }
                }
                else if (cmdDef.IsIntInt)
                {
                    var entityResult = _entity.TryGet<IntIntProperty>(command, out var entityProp);
                    if (entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED)
                    {
                        entityValue1 = entityProp?.Value1;
                        entityValue2 = entityProp?.Value2;
                        entityHasValue = true;
                        entityIsCopied = entityResult == ReturnType.COPIED;
                        entityHasDirect = entityResult == ReturnType.TRUE;
                    }
                }
                else if (cmdDef.IsString)
                {
                    var entityResult = _entity.TryGet<StringProperty>(command, out var entityProp);
                    if (entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED)
                    {
                        entityStringValue = entityProp?.Value;
                        entityHasValue = true;
                        entityIsCopied = entityResult == ReturnType.COPIED;
                        entityHasDirect = entityResult == ReturnType.TRUE;
                    }
                }
                else if (cmdDef.IsBitmask)
                {
                    var entityResult = _entity.TryGet<BitmaskProperty>(command, out var entityProp);
                    if (entityResult == ReturnType.TRUE || entityResult == ReturnType.COPIED)
                    {
                        entityBitmaskValue = entityProp?.Value;
                        entityHasValue = true;
                        entityIsCopied = entityResult == ReturnType.COPIED;
                        entityHasDirect = entityResult == ReturnType.TRUE;
                    }
                }

                // Determine effective value (entity overrides vanilla)
                bool hasValue = entityHasValue || vanillaHasValue;
                int? effectiveValue = entityHasValue ? entityValue : vanillaValue;
                bool effectiveFlagValue = entityHasValue ? entityFlagValue : vanillaFlagValue;
                // For IntIntProperty
                int effectiveValue1 = entityHasValue ? (entityValue1 ?? 0) : (vanillaValue1 ?? 0);
                int effectiveValue2 = entityHasValue ? (entityValue2 ?? 0) : (vanillaValue2 ?? 0);
                // For StringProperty
                string effectiveStringValue = entityHasValue ? entityStringValue : vanillaStringValue;
                // For BitmaskProperty
                ulong effectiveBitmaskValue = entityHasValue ? (entityBitmaskValue ?? 0) : (vanillaBitmaskValue ?? 0);

                // When showDefaults is true and no value exists, use the default from JSON
                bool usingDefault = false;
                if (!hasValue && section.ShowDefaults && cmdDef.IsInt && cmdDef.Default.HasValue)
                {
                    effectiveValue = cmdDef.Default.Value;
                    hasValue = true;
                    usingDefault = true;
                }

                // Determine modification and inheritance status
                bool isModified = false;
                bool isInherited = false;
                bool isSessionEdit = IsPropertyEditedInSession(command);

                if (cmdDef.IsFlag)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (entityFlagValue != vanillaFlagValue);
                    // Inherited if value comes from copystats or vanilla (not directly on entity)
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }
                else if (cmdDef.IsInt)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (!vanillaHasValue || entityValue != vanillaValue);
                    // Inherited if value comes from copystats or vanilla (not directly on entity)
                    // Also inherited if using default value (no actual property set)
                    isInherited = usingDefault || (!entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue)));
                }
                else if (cmdDef.IsIntInt)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (!vanillaHasValue || entityValue1 != vanillaValue1 || entityValue2 != vanillaValue2);
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }
                else if (cmdDef.IsString)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (!vanillaHasValue || entityStringValue != vanillaStringValue);
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }
                else if (cmdDef.IsBitmask)
                {
                    // Modified if entity has direct value different from vanilla
                    isModified = entityHasDirect && (!vanillaHasValue || entityBitmaskValue != vanillaBitmaskValue);
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }

                if (hasValue && (cmdDef.IsFlag ? effectiveFlagValue : true))
                {
                    PropertyItem badge;
                    if (cmdDef.IsIntInt)
                    {
                        badge = BadgeConfigLoader.CreateIntIntPropertyItem(cmdDef, effectiveValue1, effectiveValue2, isModified, isSessionEdit);
                    }
                    else if (cmdDef.IsString)
                    {
                        badge = BadgeConfigLoader.CreateStringPropertyItem(cmdDef, effectiveStringValue, isModified, isSessionEdit);
                    }
                    else if (cmdDef.IsBitmask)
                    {
                        badge = BadgeConfigLoader.CreateBitmaskPropertyItem(cmdDef, effectiveBitmaskValue, isModified, isSessionEdit);
                    }
                    else
                    {
                        badge = BadgeConfigLoader.CreatePropertyItem(cmdDef, effectiveValue, isModified, isSessionEdit);
                    }

                    // Can only remove if:
                    // 1. Section is not read-only (from JSON config)
                    // 2. Property is directly on the entity (not inherited from copystats)
                    // 3. Either entity source allows removal OR the property is a session edit
                    // 4. Not using a default value (nothing to remove)
                    bool canRemoveBasedOnSource = _source == EntitySource.FromMod
                                                  || _source == EntitySource.VanillaModified
                                                  || _source == EntitySource.New;
                    // Also allow removal if it's a session edit (user added it this session)
                    badge.CanRemove = !section.ReadOnly && entityHasDirect && !usingDefault && (canRemoveBasedOnSource || isSessionEdit);
                    badge.IsInherited = section.ReadOnly || isInherited;

                    if (valueChangedHandler != null && cmdDef.IsInt)
                    {
                        badge.ValueChanged += valueChangedHandler;
                    }

                    active.Add(badge);
                    usedCommands.Add(command);
                }
            }

            // Build available list (excluding already used commands unless multiples allowed)
            // Ref types and commands with AllowMultiple=true are always available for adding more
            if (!section.ReadOnly)
            {
                foreach (var cmdDef in section.Commands)
                {
                    if (BadgeConfigLoader.TryGetCommand(cmdDef, out var command))
                    {
                        // For ref types or AllowMultiple: always show in available list (can add multiple)
                        // For other types: only show if not already used
                        if (cmdDef.IsRef || cmdDef.AllowMultiple || !usedCommands.Contains(command))
                        {
                            available.Add(BadgeConfigLoader.CreateAvailableItem(cmdDef));
                        }
                    }
                }
            }

            return (active, available);
        }

        /// <summary>
        /// Builds reference badges for a command that references other entities.
        /// Handles multi-value refs with layered access (vanilla -> mod -> session).
        /// Supports both StringOrIDRef and other Reference types (MonsterOrMontagRef, etc.)
        /// </summary>
        private List<PropertyItem> BuildReferenceBadges(BadgeCommand cmdDef, Command command, bool sectionReadOnly, IDEntity vanillaEntity)
        {
            var badges = new List<PropertyItem>();
            var seenIds = new HashSet<int>();
            var vanillaIds = new HashSet<int>();

            // Get the entity type for name resolution
            var entityType = BadgeConfigLoader.GetEntityTypeFromRefType(cmdDef.RefType);

            // Get available references for the dropdown (used for editable badges)
            var availableRefs = GetAvailableReferencesForType(cmdDef.RefType);

            bool canRemoveBasedOnSource = _source == EntitySource.FromMod
                                          || _source == EntitySource.VanillaModified
                                          || _source == EntitySource.New;

            // Layer 1: Get references from vanilla entity (inherited)
            if (vanillaEntity != null)
            {
                foreach (var prop in vanillaEntity.GetMultiple(command))
                {
                    if (!TryExtractRefInfo(prop, entityType, out int refId, out string refName))
                        continue;

                    vanillaIds.Add(refId);

                    // Skip if we're also looking at the current entity and it's the same ref
                    // (will be handled in Layer 2 as direct)
                    if (vanillaEntity != _entity && HasDirectRefProperty(command, refId))
                        continue;

                    seenIds.Add(refId);

                    bool isInherited = vanillaEntity != _entity;
                    var badge = BadgeConfigLoader.CreateReferencePropertyItem(
                        cmdDef,
                        refId,
                        refName,
                        isModified: false,
                        isSessionEdit: false);

                    badge.IsInherited = sectionReadOnly || isInherited;
                    badge.CanRemove = !sectionReadOnly && !isInherited && canRemoveBasedOnSource;

                    // Set available references for editable badges
                    if (!badge.IsInherited && badge.CanRemove && availableRefs != null)
                    {
                        badge.AvailableReferences = availableRefs;
                    }

                    badges.Add(badge);
                }

                // Also check vanilla's copystats chain
                AddRefsFromCopystatsChain(vanillaEntity, cmdDef, command, entityType, sectionReadOnly, badges, seenIds, new HashSet<IDEntity> { vanillaEntity }, availableRefs);
            }

            // Layer 2: Get references from current entity (direct mod properties)
            if (_entity != vanillaEntity)
            {
                foreach (var prop in _entity.GetMultiple(command))
                {
                    if (!TryExtractRefInfo(prop, entityType, out int refId, out string refName))
                        continue;

                    bool isSessionEdit = IsPropertyEditedInSession(command);
                    bool isModified = !vanillaIds.Contains(refId);

                    if (seenIds.Contains(refId))
                    {
                        // Update existing badge to mark as modified
                        var existingBadge = badges.Find(b => b.ReferenceId == refId);
                        if (existingBadge != null)
                        {
                            existingBadge.IsModified = true;
                            existingBadge.IsInherited = false;
                            existingBadge.IsSessionEdit = isSessionEdit;
                            existingBadge.CanRemove = !sectionReadOnly && (canRemoveBasedOnSource || isSessionEdit);

                            // Set available references for editable badges
                            if (existingBadge.CanRemove && availableRefs != null)
                            {
                                existingBadge.AvailableReferences = availableRefs;
                            }
                        }
                    }
                    else
                    {
                        seenIds.Add(refId);

                        var badge = BadgeConfigLoader.CreateReferencePropertyItem(
                            cmdDef,
                            refId,
                            refName,
                            isModified: isModified,
                            isSessionEdit: isSessionEdit);

                        badge.IsInherited = false;
                        badge.CanRemove = !sectionReadOnly && (canRemoveBasedOnSource || isSessionEdit);

                        // Set available references for editable badges
                        if (badge.CanRemove && availableRefs != null)
                        {
                            badge.AvailableReferences = availableRefs;
                        }

                        badges.Add(badge);
                    }
                }

                // Also check mod entity's copystats chain
                AddRefsFromCopystatsChain(_entity, cmdDef, command, entityType, sectionReadOnly, badges, seenIds, new HashSet<IDEntity> { _entity, vanillaEntity }, availableRefs);
            }

            return badges;
        }

        /// <summary>
        /// Extracts reference ID and name from a property.
        /// Handles StringOrIDRef, MonsterOrMontagRef, and other Reference types.
        /// </summary>
        private bool TryExtractRefInfo(Property prop, EntityType? entityType, out int refId, out string refName)
        {
            refId = 0;
            refName = null;

            // Try StringOrIDRef first (has direct ID/Name access)
            if (prop is StringOrIDRef stringOrIdRef && stringOrIdRef.HasValue)
            {
                refId = stringOrIdRef.ID;
                if (entityType.HasValue)
                {
                    refName = GetEntityName(entityType.Value, refId);
                }
                refName ??= stringOrIdRef.Name;
                return true;
            }

            // Handle MonsterOrMontagRef specifically (wraps MonsterRef or MontagIDRef)
            if (prop is MonsterOrMontagRef monsterOrMontagRef)
            {
                // Try to get the wrapped MonsterRef
                if (monsterOrMontagRef.MonsterRef != null && monsterOrMontagRef.MonsterRef.HasValue)
                {
                    refId = monsterOrMontagRef.MonsterRef.ID;
                    if (entityType.HasValue)
                    {
                        refName = GetEntityName(entityType.Value, refId);
                    }
                    refName ??= monsterOrMontagRef.MonsterRef.Name;
                    return true;
                }
                // Try to get the wrapped MontagIDRef (negative IDs = montag)
                if (monsterOrMontagRef.MontagRef != null && monsterOrMontagRef.MontagRef.HasValue)
                {
                    refId = monsterOrMontagRef.MontagRef.ID;
                    // Montags are negative IDs, display as "Montag #X"
                    refName = $"Montag #{Math.Abs(refId)}";
                    return true;
                }
            }

            // Try any Reference type via TryGetEntity (requires resolution)
            if (prop is Reference reference && reference.TryGetEntity(out IDEntity referencedEntity) && referencedEntity != null)
            {
                refId = referencedEntity.ID;
                refName = referencedEntity.Name;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Helper to check if the current entity has a direct reference property with the given ID.
        /// </summary>
        private bool HasDirectRefProperty(Command command, int targetId)
        {
            foreach (var prop in _entity.GetMultiple(command))
            {
                if (TryExtractRefInfo(prop, null, out int refId, out _) && refId == targetId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Helper to add refs from a copystats chain.
        /// </summary>
        private void AddRefsFromCopystatsChain(
            IDEntity source,
            BadgeCommand cmdDef,
            Command command,
            EntityType? entityType,
            bool sectionReadOnly,
            List<PropertyItem> badges,
            HashSet<int> seenIds,
            HashSet<IDEntity> visited,
            IEnumerable<ReferenceItem> availableRefs = null)
        {
            if (source == null)
                return;

            if (!source.TryGetCopyFrom(out var copyFrom) || copyFrom == null || visited.Contains(copyFrom))
                return;

            visited.Add(copyFrom);

            foreach (var prop in copyFrom.GetMultiple(command))
            {
                if (!TryExtractRefInfo(prop, entityType, out int refId, out string refName))
                    continue;

                if (!seenIds.Contains(refId))
                {
                    seenIds.Add(refId);

                    var badge = BadgeConfigLoader.CreateReferencePropertyItem(
                        cmdDef,
                        refId,
                        refName,
                        isModified: false,
                        isSessionEdit: false);

                    badge.IsInherited = true;
                    badge.CanRemove = false; // Inherited from copystats, can't remove
                    // Note: Inherited badges don't get AvailableReferences since they can't be edited

                    badges.Add(badge);
                }
            }

            // Recurse through copystats chain
            AddRefsFromCopystatsChain(copyFrom, cmdDef, command, entityType, sectionReadOnly, badges, seenIds, visited, availableRefs);
        }

        /// <summary>
        /// Helper method to add a badge (property) to the entity.
        /// Handles flag, int, and reference property types.
        /// For reference types (multi-value), this adds a new property instance.
        /// </summary>
        protected void AddBadgeProperty(AvailablePropertyItem badge)
        {
            if (badge.IsReference)
            {
                // Reference types are multi-value - use entity's property map for correct type
                AddPropertyFromMap(badge.Command, badge.DefaultValue ?? 0);
            }
            else if (badge.DefaultValue.HasValue)
            {
                SetIntProperty(badge.Command, badge.DefaultValue.Value);
            }
            else
            {
                SetCommandProperty(badge.Command, true);
            }
        }

        /// <summary>
        /// Adds a property using the entity's property map to get the correct type.
        /// This ensures reference properties (MonsterRef, WeaponRef, etc.) are created
        /// with the correct type rather than as IntProperty.
        /// </summary>
        protected void AddPropertyFromMap(Command command, int value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var propertyMap = _entity.GetPropertyMap();

            Property property;
            if (propertyMap.TryGetValue(command, out var factory))
            {
                // Use the entity's factory to create the correct property type
                property = factory();
                property.Parent = _entity;
                property.Parse(command, value.ToString(), "");
            }
            else
            {
                // Fallback to IntProperty for commands not in the property map
                property = IntProperty.Create(command, _entity, value);
            }

            // Use CommandHistory for undo/redo support
            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, property, $"Add {command}");
                _history.Execute(cmd);
            }
            else
            {
                // Fallback: direct modification (no undo support)
                _entity.AddProperty(property);
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Adds a new int property to the entity (for multi-value commands).
        /// Unlike SetIntProperty which replaces, this always adds a new instance.
        /// </summary>
        protected void AddIntProperty(Command command, int value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var property = IntProperty.Create(command, _entity, value);

            // Use CommandHistory for undo/redo support
            if (_history != null)
            {
                var cmd = new AddPropertyCommand(_entity, property, $"Add {command}");
                _history.Execute(cmd);
            }
            else
            {
                // Fallback: direct modification (no undo support)
                _entity.AddProperty(property);
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Helper method to remove a badge (property) from the entity.
        /// Handles flag, int, and reference property types.
        /// For reference types (multi-value), removes the specific instance matching the reference ID.
        /// </summary>
        protected void RemoveBadgeProperty(PropertyItem badge)
        {
            if (badge.IsReference)
            {
                // Reference types are multi-value - find and remove the specific instance
                RemoveIntPropertyByValue(badge.Command, badge.ReferenceId);
            }
            else if (badge.HasValue)
            {
                SetIntProperty(badge.Command, null);
            }
            else
            {
                SetCommandProperty(badge.Command, false);
            }
        }

        /// <summary>
        /// Removes a specific property instance by matching command and value.
        /// Used for multi-value commands where we need to remove a specific entry.
        /// Handles both IntProperty and Reference types (StringOrIDRef, MonsterOrMontagRef, etc.).
        /// </summary>
        protected void RemoveIntPropertyByValue(Command command, int value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Find the specific property with matching command and value
            Property propertyToRemove = null;
            foreach (var prop in _entity.GetMultiple(command))
            {
                // Check IntProperty first
                if (prop is IntProperty intProp && intProp.Value == value)
                {
                    propertyToRemove = prop;
                    break;
                }

                // Check StringOrIDRef (base class for most single-entity references)
                if (prop is StringOrIDRef stringOrIdRef && stringOrIdRef.HasValue && stringOrIdRef.ID == value)
                {
                    propertyToRemove = prop;
                    break;
                }

                // Check MonsterOrMontagRef (wrapper for monster or montag references)
                if (prop is MonsterOrMontagRef monsterOrMontagRef)
                {
                    if (monsterOrMontagRef.MonsterRef != null &&
                        monsterOrMontagRef.MonsterRef.HasValue &&
                        monsterOrMontagRef.MonsterRef.ID == value)
                    {
                        propertyToRemove = prop;
                        break;
                    }
                    if (monsterOrMontagRef.MontagRef != null &&
                        monsterOrMontagRef.MontagRef.HasValue &&
                        monsterOrMontagRef.MontagRef.ID == value)
                    {
                        propertyToRemove = prop;
                        break;
                    }
                }
            }

            if (propertyToRemove == null)
                return;

            // Use CommandHistory for undo/redo support
            if (_history != null)
            {
                var cmd = new RemovePropertyCommand(_entity, propertyToRemove, $"Remove {command}");
                _history.Execute(cmd);
            }
            else
            {
                // Fallback: direct modification (no undo support)
                _entity.RemoveProperty(propertyToRemove);
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Creates a standard badge value changed handler that updates the property.
        /// Use this when wiring up badge collections.
        /// </summary>
        protected EventHandler<int> CreateBadgeValueChangedHandler()
        {
            return (sender, newValue) =>
            {
                if (sender is PropertyItem badge)
                {
                    SetIntProperty(badge.Command, newValue);
                    // Update the badge's session edit indicator
                    badge.IsSessionEdit = IsPropertyEditedInSession(badge.Command);
                    badge.IsModified = true;
                }
            };
        }

        /// <summary>
        /// Creates a RelayCommand for removing badges from a section.
        /// </summary>
        protected RelayCommand<PropertyItem> CreateRemoveBadgeCommand(Action refreshAction)
        {
            return new RelayCommand<PropertyItem>(badge =>
            {
                RemoveBadgeProperty(badge);
                refreshAction?.Invoke();
            });
        }

        /// <summary>
        /// Creates a RelayCommand for adding badges to a section.
        /// </summary>
        protected RelayCommand<AvailablePropertyItem> CreateAddBadgeCommand(Action refreshAction)
        {
            return new RelayCommand<AvailablePropertyItem>(badge =>
            {
                AddBadgeProperty(badge);
                refreshAction?.Invoke();
            });
        }

        // ========================================
        // Generic Entity Resolution (Layered Lookup)
        // ========================================

        /// <summary>
        /// Resolves an entity reference by type and ID, cascading through layers:
        /// 1. Current mod's database
        /// 2. Vanilla database
        /// Returns null if not found in any layer.
        /// </summary>
        protected IDEntity ResolveEntityReference(EntityType type, int id)
        {
            // Layer 1: Try current mod
            var mod = _entity.ParentMod;
            if (mod?.Database.TryGetValue(type, out var modSet) == true)
            {
                if (modSet.TryGetValue(id, out var entity))
                    return entity;
            }

            // Layer 2: Fall back to vanilla
            if (VanillaLoader.Vanilla?.Database.TryGetValue(type, out var vanillaSet) == true)
            {
                if (vanillaSet.TryGetValue(id, out var entity))
                    return entity;
            }

            return null;
        }

        /// <summary>
        /// Gets an entity's name by type and ID, using layered resolution.
        /// Returns null if entity not found.
        /// </summary>
        protected string GetEntityName(EntityType type, int id)
        {
            return ResolveEntityReference(type, id)?.Name;
        }

        /// <summary>
        /// Gets a reference property's resolved name, with fallback to layered lookup if unresolved.
        /// </summary>
        protected string GetReferenceName(StringOrIDRef reference, EntityType entityType)
        {
            if (reference == null || !reference.HasValue)
                return null;

            // Try the resolved entity first
            if (reference.Entity != null)
                return reference.Entity.Name;

            // Fall back to layered lookup
            return GetEntityName(entityType, reference.ID);
        }

        /// <summary>
        /// Gets all multi-value properties of a given type with layered access.
        /// Returns properties from vanilla first (marked as inherited), then from mod (marked as direct).
        /// </summary>
        /// <typeparam name="TRef">The reference property type (e.g., WeaponRef, ArmorRef)</typeparam>
        /// <param name="command">The command to look for (e.g., Command.WEAPON)</param>
        /// <param name="entityType">The entity type for name resolution fallback</param>
        /// <param name="getId">Function to extract ID from the reference</param>
        /// <returns>List of tuples: (ID, Name, IsInherited from vanilla, IsModified from mod)</returns>
        protected List<(int Id, string Name, bool IsInherited, bool IsModified, bool IsSessionEdit)>
            GetLayeredReferenceList<TRef>(Command command, EntityType entityType, Func<TRef, int> getId)
            where TRef : StringOrIDRef
        {
            var results = new List<(int Id, string Name, bool IsInherited, bool IsModified, bool IsSessionEdit)>();
            var knownIds = new HashSet<int>();
            var vanillaEntity = GetVanillaEntity();
            var vanillaIds = new HashSet<int>();

            // Layer 1: Get IDs from vanilla entity (base layer)
            if (vanillaEntity != null)
            {
                foreach (var prop in vanillaEntity.GetMultiple(command))
                {
                    if (prop is TRef refProp && refProp.HasValue)
                    {
                        var id = getId(refProp);
                        vanillaIds.Add(id);

                        // Only add if not also directly on current entity (will handle in Layer 2)
                        if (vanillaEntity == _entity || !HasDirectProperty<TRef>(command, id, getId))
                        {
                            knownIds.Add(id);
                            var name = GetReferenceName(refProp, entityType);
                            results.Add((id, name, IsInherited: vanillaEntity != _entity, IsModified: false, IsSessionEdit: false));
                        }
                    }
                }

                // Also check vanilla's copystats chain
                if (vanillaEntity.TryGetCopyFrom(out var vanillaCopy) && vanillaCopy != null)
                {
                    AddFromCopystatsChain(vanillaCopy, command, entityType, getId, results, knownIds, new HashSet<IDEntity> { vanillaEntity });
                }
            }

            // Layer 2: Get from current entity (mod layer) - only if different from vanilla
            if (_entity != vanillaEntity)
            {
                foreach (var prop in _entity.GetMultiple(command))
                {
                    if (prop is TRef refProp && refProp.HasValue)
                    {
                        var id = getId(refProp);
                        var name = GetReferenceName(refProp, entityType);
                        bool isSessionEdit = IsPropertyEditedInSession(command);

                        if (knownIds.Contains(id))
                        {
                            // Update existing entry to mark as modified (exists in both vanilla and mod)
                            var index = results.FindIndex(r => r.Id == id);
                            if (index >= 0)
                            {
                                results[index] = (id, name, IsInherited: false, IsModified: true, isSessionEdit);
                            }
                        }
                        else
                        {
                            // New in mod (not in vanilla)
                            knownIds.Add(id);
                            bool isModified = !vanillaIds.Contains(id);
                            results.Add((id, name, IsInherited: false, IsModified: isModified, isSessionEdit));
                        }
                    }
                }

                // Check mod entity's copystats chain
                if (_entity.TryGetCopyFrom(out var modCopy) && modCopy != null)
                {
                    AddFromCopystatsChain(modCopy, command, entityType, getId, results, knownIds, new HashSet<IDEntity> { _entity, vanillaEntity });
                }
            }

            return results;
        }

        /// <summary>
        /// Helper to check if entity has a direct property (not from copystats) with given ID.
        /// </summary>
        private bool HasDirectProperty<TRef>(Command command, int targetId, Func<TRef, int> getId) where TRef : StringOrIDRef
        {
            foreach (var prop in _entity.GetMultiple(command))
            {
                if (prop is TRef refProp && refProp.HasValue && getId(refProp) == targetId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Helper to add references from a copystats chain.
        /// </summary>
        private void AddFromCopystatsChain<TRef>(
            IDEntity source,
            Command command,
            EntityType entityType,
            Func<TRef, int> getId,
            List<(int Id, string Name, bool IsInherited, bool IsModified, bool IsSessionEdit)> results,
            HashSet<int> knownIds,
            HashSet<IDEntity> visited) where TRef : StringOrIDRef
        {
            if (source == null || visited.Contains(source))
                return;
            visited.Add(source);

            foreach (var prop in source.GetMultiple(command))
            {
                if (prop is TRef refProp && refProp.HasValue)
                {
                    var id = getId(refProp);
                    if (!knownIds.Contains(id))
                    {
                        knownIds.Add(id);
                        var name = GetReferenceName(refProp, entityType);
                        results.Add((id, name, IsInherited: true, IsModified: false, IsSessionEdit: false));
                    }
                }
            }

            // Recurse through copystats chain
            if (source.TryGetCopyFrom(out var nextCopy) && nextCopy != null)
            {
                AddFromCopystatsChain(nextCopy, command, entityType, getId, results, knownIds, visited);
            }
        }

        /// <summary>
        /// Entity ID.
        /// </summary>
        public int ID => _entity.ID;

        /// <summary>
        /// Display name for lists and headers.
        /// </summary>
        public virtual string DisplayName
        {
            get
            {
                var name = _entity.Name;

                // For VanillaModified entities, fall back to vanilla name if not in mod
                if (string.IsNullOrEmpty(name) && _source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    name = vanillaEntity?.Name;
                }

                return name ?? $"#{_entity.ID}";
            }
        }

        /// <summary>
        /// Entity name (editable).
        /// </summary>
        public string Name
        {
            get
            {
                var name = _entity.Name;

                // For VanillaModified entities, fall back to vanilla name if not in mod
                if (string.IsNullOrEmpty(name) && _source == EntitySource.VanillaModified)
                {
                    var vanillaEntity = GetVanillaEntity();
                    name = vanillaEntity?.Name;
                }

                return name ?? string.Empty;
            }
            set
            {
                if (_entity.Name != value)
                {
                    // TODO: Use SetNameCommand for undo support
                    _entity.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        /// <summary>
        /// The source/origin of this entity.
        /// </summary>
        public EntitySource Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    _source = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsVanilla));
                    OnPropertyChanged(nameof(IsFromMod));
                    OnPropertyChanged(nameof(IsModified));
                    OnPropertyChanged(nameof(IsNew));
                    OnPropertyChanged(nameof(SourceLabel));
                }
            }
        }

        /// <summary>
        /// True if this is a vanilla (base game) entity (modified or not).
        /// </summary>
        public bool IsVanilla => _source == EntitySource.Vanilla || _source == EntitySource.VanillaModified;

        /// <summary>
        /// True if this entity was created by the loaded mod (not vanilla).
        /// </summary>
        public bool IsFromMod => _source == EntitySource.FromMod;

        /// <summary>
        /// True if this vanilla entity has been modified by the mod or current session.
        /// </summary>
        public bool IsModified => _source == EntitySource.VanillaModified || _hasSessionChanges;

        /// <summary>
        /// True if this entity was created in the current editing session.
        /// </summary>
        public bool IsNew => _source == EntitySource.New;

        /// <summary>
        /// True if changes were made to this entity in the current session.
        /// </summary>
        public bool HasSessionChanges
        {
            get => _hasSessionChanges;
            set
            {
                if (_hasSessionChanges != value)
                {
                    _hasSessionChanges = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        /// <summary>
        /// Short label describing the entity source for display.
        /// </summary>
        public string SourceLabel => _source switch
        {
            EntitySource.Vanilla => "Vanilla",
            EntitySource.VanillaModified => "Modified",
            EntitySource.FromMod => "Mod",
            EntitySource.New => "New",
            _ => ""
        };

        /// <summary>
        /// Command history for undo/redo support.
        /// </summary>
        public CommandHistory History => _history;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ========================================
        // Property Access Helpers
        // ========================================

        /// <summary>
        /// Generic property getter with VanillaModified fallback support.
        /// Returns the property from the current entity, or falls back to vanilla for VanillaModified entities.
        /// This is the base method for all property access with layered fallback.
        /// </summary>
        protected T GetProperty<T>(Command command) where T : Property, new()
        {
            var result = _entity.TryGet<T>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return prop;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<T>(command, out var vanillaProp);
                    if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                    {
                        return vanillaProp;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a property exists, with VanillaModified fallback support.
        /// </summary>
        protected bool HasProperty<T>(Command command) where T : Property, new()
        {
            return GetProperty<T>(command) != null;
        }

        protected string GetStringProperty(Command command)
        {
            return GetProperty<StringProperty>(command)?.Value ?? string.Empty;
        }

        protected void SetStringProperty(Command command, string value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Use CommandHistory for undo/redo support
            if (_history != null)
            {
                var cmd = new SetStringPropertyCommand(_entity, command, value);
                _history.Execute(cmd);
            }
            else
            {
                // Fallback: direct modification (no undo support)
                _entity.Set<StringProperty>(command, p => p.Value = value);
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
            // Notify related session edit and modified properties
            if (propertyName != null)
            {
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Modified");
            }
        }

        protected int? GetIntProperty(Command command)
        {
            return GetProperty<IntProperty>(command)?.Value;
        }

        /// <summary>
        /// Gets a reference property with VanillaModified fallback support.
        /// Alias for GetProperty&lt;T&gt; for backwards compatibility.
        /// </summary>
        protected T GetReferenceProperty<T>(Command command) where T : Property, new()
        {
            return GetProperty<T>(command);
        }

        /// <summary>
        /// Checks if a reference property exists, with VanillaModified fallback support.
        /// Alias for HasProperty&lt;T&gt; for backwards compatibility.
        /// </summary>
        protected bool HasReferenceProperty<T>(Command command) where T : Property, new()
        {
            return HasProperty<T>(command);
        }

        protected void SetIntProperty(Command command, int? value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Use CommandHistory for undo/redo support
            if (_history != null)
            {
                if (value.HasValue)
                {
                    var cmd = new SetIntPropertyCommand(_entity, command, value.Value);
                    _history.Execute(cmd);
                }
                else
                {
                    // Removing property - get the existing property first for undo support
                    var result = _entity.TryGet<IntProperty>(command, out var existingProp, checkCopy: false);
                    if (result == ReturnType.TRUE && existingProp != null)
                    {
                        var cmd = new RemovePropertyCommand(_entity, existingProp, $"Remove {command}");
                        _history.Execute(cmd);
                    }
                    // If property doesn't exist, nothing to remove
                }
            }
            else
            {
                // Fallback: direct modification (no undo support)
                if (value.HasValue)
                {
                    _entity.Set<IntProperty>(command, p => p.Value = value.Value);
                }
                else
                {
                    _entity.Remove<IntProperty>(command);
                }
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
            // Notify related session edit and modified properties
            if (propertyName != null)
            {
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Modified");
            }
        }

        protected bool GetCommandProperty(Command command)
        {
            return HasProperty<CommandProperty>(command);
        }

        protected void SetCommandProperty(Command command, bool value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            // Use CommandHistory for undo/redo support
            if (_history != null)
            {
                var cmd = new SetCommandPropertyCommand(_entity, command, value);
                _history.Execute(cmd);
            }
            else
            {
                // Fallback: direct modification (no undo support)
                if (value)
                {
                    _entity.SetCommand<CommandProperty>(command);
                }
                else
                {
                    _entity.Remove<CommandProperty>(command);
                }
            }

            HasSessionChanges = true;
            OnPropertyChanged(propertyName);
            // Notify related session edit and modified properties
            if (propertyName != null)
            {
                OnPropertyChanged($"Is{propertyName}SessionEdit");
                OnPropertyChanged($"Is{propertyName}Modified");
            }
        }

        /// <summary>
        /// Generic method to check if a property is inherited (from copystats).
        /// Returns true if the property comes from a copystats chain rather than being set directly.
        /// </summary>
        protected bool IsPropertyInherited<T>(Command command) where T : Property, new()
        {
            var result = _entity.TryGet<T>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<T>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        protected bool IsIntPropertyInherited(Command command)
        {
            return IsPropertyInherited<IntProperty>(command);
        }

        protected bool IsStringPropertyInherited(Command command)
        {
            return IsPropertyInherited<StringProperty>(command);
        }

        protected bool IsCommandPropertyInherited(Command command)
        {
            return IsPropertyInherited<CommandProperty>(command);
        }

        // ========================================
        // Modification Tracking Infrastructure
        // ========================================

        /// <summary>
        /// Sets the ChangesMod reference for session edit tracking.
        /// </summary>
        public void SetChangesMod(ChangesMod changesMod)
        {
            _changesMod = changesMod;
        }

        // ========================================
        // Original Value Helpers (before session edits)
        // ========================================

        /// <summary>
        /// Generic method to get the original property (from mod or vanilla, before any session edits).
        /// Used to detect when a value is reset to its original state.
        /// </summary>
        protected T GetOriginalProperty<T>(Command command) where T : Property, new()
        {
            // For vanilla entities, the original is the vanilla value
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<T>(command, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop;
            }

            // For mod entities (FromMod), check if there's a value in the loaded mod
            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<T>(command, out var prop);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return prop;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if an original property exists (from mod or vanilla).
        /// </summary>
        protected bool HasOriginalProperty<T>(Command command) where T : Property, new()
        {
            return GetOriginalProperty<T>(command) != null;
        }

        protected int? GetOriginalIntValue(Command command)
        {
            return GetOriginalProperty<IntProperty>(command)?.Value;
        }

        protected string GetOriginalStringValue(Command command)
        {
            return GetOriginalProperty<StringProperty>(command)?.Value ?? string.Empty;
        }

        protected bool GetOriginalCommandValue(Command command)
        {
            return HasOriginalProperty<CommandProperty>(command);
        }

        /// <summary>
        /// Resets an int property to its original value (from mod or vanilla).
        /// </summary>
        public void ResetIntProperty(Command command, string propertyName)
        {
            var original = GetOriginalIntValue(command);
            if (original.HasValue)
            {
                _entity.Set<IntProperty>(command, p => p.Value = original.Value);
            }
            else
            {
                _entity.Remove<IntProperty>(command);
            }
            _changesMod?.RevertPropertyChange(_entity, command);
            OnPropertyChanged(propertyName);
            OnPropertyChanged($"Is{propertyName}SessionEdit");
            OnPropertyChanged($"Is{propertyName}Modified");
        }

        /// <summary>
        /// Resets a string property to its original value.
        /// </summary>
        public void ResetStringProperty(Command command, string propertyName)
        {
            var original = GetOriginalStringValue(command);
            if (!string.IsNullOrEmpty(original))
            {
                _entity.Set<StringProperty>(command, p => p.Value = original);
            }
            else
            {
                _entity.Remove<StringProperty>(command);
            }
            _changesMod?.RevertPropertyChange(_entity, command);
            OnPropertyChanged(propertyName);
            OnPropertyChanged($"Is{propertyName}SessionEdit");
            OnPropertyChanged($"Is{propertyName}Modified");
        }

        /// <summary>
        /// Resets a command (boolean) property to its original value.
        /// </summary>
        public void ResetCommandProperty(Command command, string propertyName)
        {
            var original = GetOriginalCommandValue(command);
            if (original)
            {
                _entity.SetCommand<CommandProperty>(command);
            }
            else
            {
                _entity.Remove<CommandProperty>(command);
            }
            _changesMod?.RevertPropertyChange(_entity, command);
            OnPropertyChanged(propertyName);
            OnPropertyChanged($"Is{propertyName}SessionEdit");
            OnPropertyChanged($"Is{propertyName}Modified");
        }

        /// <summary>
        /// Checks if a property has a session edit that can be reset.
        /// Works for any property type (int, string, command).
        /// </summary>
        public bool CanResetProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

        // Type-specific aliases for backwards compatibility
        public bool CanResetIntProperty(Command command) => CanResetProperty(command);
        public bool CanResetStringProperty(Command command) => CanResetProperty(command);
        public bool CanResetCommandProperty(Command command) => CanResetProperty(command);

        /// <summary>
        /// Gets the vanilla version of this entity for comparison.
        /// Returns null if entity doesn't exist in vanilla.
        /// </summary>
        protected IDEntity GetVanillaEntity()
        {
            if (_vanillaEntityResolved)
                return _vanillaEntity;

            _vanillaEntityResolved = true;

            // New entities have no vanilla counterpart
            if (_source == EntitySource.New || _source == EntitySource.FromMod)
            {
                _vanillaEntity = null;
                return null;
            }

            // Look up vanilla entity by type and ID
            var entityType = GetEntityTypeFromEntity(_entity);
            if (entityType.HasValue && VanillaLoader.Vanilla?.Database.TryGetValue(entityType.Value, out var vanillaSet) == true)
            {
                vanillaSet.TryGetValue(_entity.ID, out var vanillaEntity);
                _vanillaEntity = vanillaEntity;
            }

            return _vanillaEntity;
        }

        /// <summary>
        /// Gets the EntityType for an entity using pattern matching.
        /// </summary>
        private static EntityType? GetEntityTypeFromEntity(IDEntity entity)
        {
            return entity switch
            {
                Monster _ => EntityType.MONSTER,
                Weapon _ => EntityType.WEAPON,
                Armor _ => EntityType.ARMOR,
                Spell _ => EntityType.SPELL,
                Item _ => EntityType.ITEM,
                Site _ => EntityType.SITE,
                Nation _ => EntityType.NATION,
                Event _ => EntityType.EVENT,
                Mercenary _ => EntityType.MERCENARY,
                Nametype _ => EntityType.NAMETYPE,
                Poptype _ => EntityType.POPTYPE,
                _ => null
            };
        }

        /// <summary>
        /// Generic method to check if a property differs from vanilla value.
        /// Returns true if the value is different from vanilla (mod change).
        /// </summary>
        /// <param name="command">The command to check</param>
        /// <param name="valuesEqual">Optional function to compare property values. If null, only presence is checked.</param>
        protected bool IsPropertyModifiedFromVanilla<T>(Command command, Func<T, T, bool> valuesEqual = null) where T : Property, new()
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                // No vanilla entity - if property exists, it's "modified" (new)
                var result = _entity.TryGet<T>(command, out _);
                return result == ReturnType.TRUE;
            }

            // Get current value from mod entity
            var currentResult = _entity.TryGet<T>(command, out var currentProp);
            var vanillaResult = vanillaEntity.TryGet<T>(command, out var vanillaProp);

            bool currentHasValue = currentResult == ReturnType.TRUE || currentResult == ReturnType.COPIED;
            bool vanillaHasValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;

            // For VanillaModified entities: if mod doesn't have the property,
            // the effective value is vanilla, so it's NOT modified
            if (_source == EntitySource.VanillaModified && !currentHasValue)
                return false;

            if (currentHasValue != vanillaHasValue)
                return true;

            if (!currentHasValue && !vanillaHasValue)
                return false;

            // If no value comparator provided, presence check is sufficient (for CommandProperty)
            if (valuesEqual == null)
                return false;

            // Values differ if they're NOT equal
            return !valuesEqual(currentProp, vanillaProp);
        }

        protected bool IsIntPropertyModifiedFromVanilla(Command command)
        {
            return IsPropertyModifiedFromVanilla<IntProperty>(command, (a, b) => a?.Value == b?.Value);
        }

        protected bool IsStringPropertyModifiedFromVanilla(Command command)
        {
            return IsPropertyModifiedFromVanilla<StringProperty>(command, (a, b) => a?.Value == b?.Value);
        }

        protected bool IsCommandPropertyModifiedFromVanilla(Command command)
        {
            return IsPropertyModifiedFromVanilla<CommandProperty>(command);
        }

        /// <summary>
        /// Checks if a property was edited in the current session (tracked via ChangesMod).
        /// </summary>
        protected bool IsPropertyEditedInSession(Command command)
        {
            if (_changesMod == null)
                return false;

            var entityType = GetEntityTypeFromEntity(_entity);
            if (!entityType.HasValue)
                return false;

            if (!_changesMod.TryGetChanges(entityType.Value, _entity.ID, out var changes))
                return false;

            return changes.ChangedProperties.ContainsKey(command);
        }

        /// <summary>
        /// Gets a human-readable display name for a command.
        /// </summary>
        protected virtual string GetCommandDisplayName(Command command)
        {
            // Convert command enum to display name
            // Override in subclasses for custom display names
            var name = command.ToString();
            // Convert SCREAMING_CASE to Title Case
            return System.Text.RegularExpressions.Regex.Replace(
                name.ToLower().Replace("_", " "),
                @"\b\w",
                m => m.Value.ToUpper());
        }
    }
}
