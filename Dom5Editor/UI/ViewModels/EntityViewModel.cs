using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dom5Edit;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.Data;
using Dom5Editor.EditCommands;
using Dom5Editor.UI.Controls;
using Dom5Editor.VMs;

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
                }

                // Layer 2: Get current entity value (mod + session, includes mod copystats)
                bool entityHasValue = false;
                bool entityIsCopied = false;
                bool entityHasDirect = false; // Has the property directly (not from copystats)
                int? entityValue = null;
                bool entityFlagValue = false;

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

                // Determine effective value (entity overrides vanilla)
                bool hasValue = entityHasValue || vanillaHasValue;
                int? effectiveValue = entityHasValue ? entityValue : vanillaValue;
                bool effectiveFlagValue = entityHasValue ? entityFlagValue : vanillaFlagValue;

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
                    isInherited = !entityHasDirect && (entityIsCopied || (!entityHasValue && vanillaHasValue));
                }

                if (hasValue && (cmdDef.IsFlag ? effectiveFlagValue : true))
                {
                    var badge = BadgeConfigLoader.CreatePropertyItem(cmdDef, effectiveValue, isModified, isSessionEdit);
                    // Can only remove if:
                    // 1. Section is not read-only (from JSON config)
                    // 2. Property is directly on the entity (not inherited from copystats)
                    // 3. Either entity source allows removal OR the property is a session edit
                    bool canRemoveBasedOnSource = _source == EntitySource.FromMod
                                                  || _source == EntitySource.VanillaModified
                                                  || _source == EntitySource.New;
                    // Also allow removal if it's a session edit (user added it this session)
                    badge.CanRemove = !section.ReadOnly && entityHasDirect && (canRemoveBasedOnSource || isSessionEdit);
                    badge.IsInherited = section.ReadOnly || isInherited;

                    if (valueChangedHandler != null && cmdDef.IsInt)
                    {
                        badge.ValueChanged += valueChangedHandler;
                    }

                    active.Add(badge);
                    usedCommands.Add(command);
                }
            }

            // Build available list (excluding already used commands for non-ref types)
            // Ref types are always available for adding more instances
            if (!section.ReadOnly)
            {
                foreach (var cmdDef in section.Commands)
                {
                    if (BadgeConfigLoader.TryGetCommand(cmdDef, out var command))
                    {
                        // For ref types: always show in available list (can add multiple)
                        // For flag/int types: only show if not already used
                        if (cmdDef.IsRef || !usedCommands.Contains(command))
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
        /// </summary>
        private List<PropertyItem> BuildReferenceBadges(BadgeCommand cmdDef, Command command, bool sectionReadOnly, IDEntity vanillaEntity)
        {
            var badges = new List<PropertyItem>();
            var seenIds = new HashSet<int>();
            var vanillaIds = new HashSet<int>();

            // Get the entity type for name resolution
            var entityType = BadgeConfigLoader.GetEntityTypeFromRefType(cmdDef.RefType);

            bool canRemoveBasedOnSource = _source == EntitySource.FromMod
                                          || _source == EntitySource.VanillaModified
                                          || _source == EntitySource.New;

            // Layer 1: Get references from vanilla entity (inherited)
            if (vanillaEntity != null)
            {
                foreach (var prop in vanillaEntity.GetMultiple(command))
                {
                    if (prop is StringOrIDRef refProp && refProp.HasValue)
                    {
                        var refId = refProp.ID;
                        vanillaIds.Add(refId);

                        // Skip if we're also looking at the current entity and it's the same ref
                        // (will be handled in Layer 2 as direct)
                        if (vanillaEntity != _entity && HasDirectRefProperty(command, refId))
                            continue;

                        seenIds.Add(refId);

                        // Resolve name
                        string refName = null;
                        if (entityType.HasValue)
                        {
                            refName = GetEntityName(entityType.Value, refId);
                        }
                        refName ??= refProp.Name;

                        bool isInherited = vanillaEntity != _entity;
                        var badge = BadgeConfigLoader.CreateReferencePropertyItem(
                            cmdDef,
                            refId,
                            refName,
                            isModified: false,
                            isSessionEdit: false);

                        badge.IsInherited = sectionReadOnly || isInherited;
                        badge.CanRemove = !sectionReadOnly && !isInherited && canRemoveBasedOnSource;

                        badges.Add(badge);
                    }
                }

                // Also check vanilla's copystats chain
                AddRefsFromCopystatsChain(vanillaEntity, cmdDef, command, entityType, sectionReadOnly, badges, seenIds, new HashSet<IDEntity> { vanillaEntity });
            }

            // Layer 2: Get references from current entity (direct mod properties)
            if (_entity != vanillaEntity)
            {
                foreach (var prop in _entity.GetMultiple(command))
                {
                    if (prop is StringOrIDRef refProp && refProp.HasValue)
                    {
                        var refId = refProp.ID;
                        bool isSessionEdit = IsPropertyEditedInSession(command);
                        bool isModified = !vanillaIds.Contains(refId);

                        // Resolve name
                        string refName = null;
                        if (entityType.HasValue)
                        {
                            refName = GetEntityName(entityType.Value, refId);
                        }
                        refName ??= refProp.Name;

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

                            badges.Add(badge);
                        }
                    }
                }

                // Also check mod entity's copystats chain
                AddRefsFromCopystatsChain(_entity, cmdDef, command, entityType, sectionReadOnly, badges, seenIds, new HashSet<IDEntity> { _entity, vanillaEntity });
            }

            return badges;
        }

        /// <summary>
        /// Helper to check if the current entity has a direct reference property with the given ID.
        /// </summary>
        private bool HasDirectRefProperty(Command command, int targetId)
        {
            foreach (var prop in _entity.GetMultiple(command))
            {
                if (prop is StringOrIDRef refProp && refProp.HasValue && refProp.ID == targetId)
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
            HashSet<IDEntity> visited)
        {
            if (source == null)
                return;

            if (!source.TryGetCopyFrom(out var copyFrom) || copyFrom == null || visited.Contains(copyFrom))
                return;

            visited.Add(copyFrom);

            foreach (var prop in copyFrom.GetMultiple(command))
            {
                if (prop is StringOrIDRef refProp && refProp.HasValue)
                {
                    var refId = refProp.ID;
                    if (!seenIds.Contains(refId))
                    {
                        seenIds.Add(refId);

                        // Resolve name
                        string refName = null;
                        if (entityType.HasValue)
                        {
                            refName = GetEntityName(entityType.Value, refId);
                        }
                        refName ??= refProp.Name;

                        var badge = BadgeConfigLoader.CreateReferencePropertyItem(
                            cmdDef,
                            refId,
                            refName,
                            isModified: false,
                            isSessionEdit: false);

                        badge.IsInherited = true;
                        badge.CanRemove = false; // Inherited from copystats, can't remove

                        badges.Add(badge);
                    }
                }
            }

            // Recurse through copystats chain
            AddRefsFromCopystatsChain(copyFrom, cmdDef, command, entityType, sectionReadOnly, badges, seenIds, visited);
        }

        /// <summary>
        /// Helper method to add a badge (property) to the entity.
        /// Handles both flag and int property types.
        /// </summary>
        protected void AddBadgeProperty(AvailablePropertyItem badge)
        {
            if (badge.DefaultValue.HasValue)
            {
                SetIntProperty(badge.Command, badge.DefaultValue.Value);
            }
            else
            {
                SetCommandProperty(badge.Command, true);
            }
        }

        /// <summary>
        /// Helper method to remove a badge (property) from the entity.
        /// Handles both flag and int property types.
        /// </summary>
        protected void RemoveBadgeProperty(PropertyItem badge)
        {
            if (badge.HasValue)
            {
                SetIntProperty(badge.Command, null);
            }
            else
            {
                SetCommandProperty(badge.Command, false);
            }
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

        protected string GetStringProperty(Command command)
        {
            var result = _entity.TryGet<StringProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return prop?.Value ?? string.Empty;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out var vanillaProp);
                    if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                    {
                        return vanillaProp?.Value ?? string.Empty;
                    }
                }
            }

            return string.Empty;
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
            var result = _entity.TryGet<IntProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return prop?.Value;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);
                    if (vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED)
                    {
                        return vanillaProp?.Value;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a reference property with VanillaModified fallback support.
        /// Returns the property from the current entity, or falls back to vanilla for VanillaModified entities.
        /// </summary>
        protected T GetReferenceProperty<T>(Command command) where T : Property, new()
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
        /// Checks if a reference property exists, with VanillaModified fallback support.
        /// </summary>
        protected bool HasReferenceProperty<T>(Command command) where T : Property, new()
        {
            return GetReferenceProperty<T>(command) != null;
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
            var result = _entity.TryGet<CommandProperty>(command, out var prop);
            if (result == ReturnType.TRUE || result == ReturnType.COPIED)
            {
                return true;
            }

            // For VanillaModified entities, fall back to vanilla value if not in mod
            if (_source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);
                    return vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
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

        protected bool IsIntPropertyInherited(Command command)
        {
            var result = _entity.TryGet<IntProperty>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        protected bool IsStringPropertyInherited(Command command)
        {
            var result = _entity.TryGet<StringProperty>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
        }

        protected bool IsCommandPropertyInherited(Command command)
        {
            var result = _entity.TryGet<CommandProperty>(command, out _);
            if (result == ReturnType.COPIED)
                return true;

            // For VanillaModified entities, check if vanilla value is inherited
            if (result == ReturnType.FALSE && _source == EntitySource.VanillaModified)
            {
                var vanillaEntity = GetVanillaEntity();
                if (vanillaEntity != null)
                {
                    var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);
                    return vanillaResult == ReturnType.COPIED;
                }
            }

            return false;
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
        /// Gets the original int value (from mod or vanilla, before any session edits).
        /// Used to detect when a value is reset to its original state.
        /// </summary>
        protected int? GetOriginalIntValue(Command command)
        {
            // For vanilla entities, the original is the vanilla value
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<IntProperty>(command, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value;
            }

            // For mod entities (FromMod), check if there's a value in the loaded mod
            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<IntProperty>(command, out var prop);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return prop?.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the original string value (from mod or vanilla, before any session edits).
        /// </summary>
        protected string GetOriginalStringValue(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<StringProperty>(command, out var prop);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return prop?.Value ?? string.Empty;
            }

            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<StringProperty>(command, out var prop);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return prop?.Value ?? string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the original command (boolean) value (from mod or vanilla, before any session edits).
        /// </summary>
        protected bool GetOriginalCommandValue(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity != null)
            {
                var result = vanillaEntity.TryGet<CommandProperty>(command, out _);
                if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                    return true;
            }

            if (_source == EntitySource.FromMod && _changesMod?.LoadedMod != null)
            {
                var entityType = GetEntityTypeFromEntity(_entity);
                if (entityType.HasValue &&
                    _changesMod.LoadedMod.Database.TryGetValue(entityType.Value, out var modSet) &&
                    modSet.TryGetValue(_entity.ID, out var modEntity))
                {
                    var result = modEntity.TryGet<CommandProperty>(command, out _);
                    if (result == ReturnType.TRUE || result == ReturnType.COPIED)
                        return true;
                }
            }

            return false;
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
        /// Checks if an int property has a session edit that can be reset.
        /// </summary>
        public bool CanResetIntProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

        /// <summary>
        /// Checks if a string property has a session edit that can be reset.
        /// </summary>
        public bool CanResetStringProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

        /// <summary>
        /// Checks if a command property has a session edit that can be reset.
        /// </summary>
        public bool CanResetCommandProperty(Command command)
        {
            return _changesMod?.IsPropertyChanged(_entity, command) == true;
        }

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
        /// Checks if an int property differs from vanilla value.
        /// Returns true if the value is different from vanilla (mod change).
        /// </summary>
        protected bool IsIntPropertyModifiedFromVanilla(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                // No vanilla entity - if property exists, it's "modified" (new)
                var result = _entity.TryGet<IntProperty>(command, out _);
                return result == ReturnType.TRUE;
            }

            // Get current value from mod entity
            var currentResult = _entity.TryGet<IntProperty>(command, out var currentProp);
            var vanillaResult = vanillaEntity.TryGet<IntProperty>(command, out var vanillaProp);

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

            return currentProp?.Value != vanillaProp?.Value;
        }

        /// <summary>
        /// Checks if a string property differs from vanilla value.
        /// </summary>
        protected bool IsStringPropertyModifiedFromVanilla(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                var result = _entity.TryGet<StringProperty>(command, out _);
                return result == ReturnType.TRUE;
            }

            var currentResult = _entity.TryGet<StringProperty>(command, out var currentProp);
            var vanillaResult = vanillaEntity.TryGet<StringProperty>(command, out var vanillaProp);

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

            return currentProp?.Value != vanillaProp?.Value;
        }

        /// <summary>
        /// Checks if a command (boolean) property differs from vanilla.
        /// </summary>
        protected bool IsCommandPropertyModifiedFromVanilla(Command command)
        {
            var vanillaEntity = GetVanillaEntity();
            if (vanillaEntity == null)
            {
                var result = _entity.TryGet<CommandProperty>(command, out _);
                return result == ReturnType.TRUE;
            }

            var currentResult = _entity.TryGet<CommandProperty>(command, out _);
            var vanillaResult = vanillaEntity.TryGet<CommandProperty>(command, out _);

            bool currentHasValue = currentResult == ReturnType.TRUE || currentResult == ReturnType.COPIED;
            bool vanillaHasValue = vanillaResult == ReturnType.TRUE || vanillaResult == ReturnType.COPIED;

            // For VanillaModified entities: if mod doesn't have the property,
            // the effective value is vanilla, so it's NOT modified
            if (_source == EntitySource.VanillaModified && !currentHasValue)
                return false;

            return currentHasValue != vanillaHasValue;
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
        /// Checks if a property is modified from vanilla OR edited in session.
        /// This is the general "is modified" check for UI highlighting.
        /// </summary>
        protected bool IsPropertyModified(Command command, PropertyType propertyType)
        {
            // Check session edits first (more recent changes)
            if (IsPropertyEditedInSession(command))
                return true;

            // Check mod vs vanilla
            return propertyType switch
            {
                PropertyType.Int => IsIntPropertyModifiedFromVanilla(command),
                PropertyType.String => IsStringPropertyModifiedFromVanilla(command),
                PropertyType.Command => IsCommandPropertyModifiedFromVanilla(command),
                _ => false
            };
        }

        /// <summary>
        /// Property types for modification checking.
        /// </summary>
        protected enum PropertyType
        {
            Int,
            String,
            Command
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
