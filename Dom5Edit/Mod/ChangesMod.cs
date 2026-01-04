using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// Tracks all changes made during an editing session.
    /// Stores deltas from vanilla and/or loaded mod state.
    /// Supports export as standalone changes or merged with loaded mod.
    /// </summary>
    public class ChangesMod
    {
        /// <summary>
        /// The loaded mod being edited, if any. Null if editing vanilla only.
        /// </summary>
        public Mod LoadedMod { get; set; }

        /// <summary>
        /// Changes to existing entities (keyed by EntityType and ID).
        /// </summary>
        private readonly Dictionary<(EntityType, int), EntityChanges> _entityChanges
            = new Dictionary<(EntityType, int), EntityChanges>();

        /// <summary>
        /// Entities that have been removed from the loaded mod.
        /// Only valid when LoadedMod is set. Cannot remove vanilla entities.
        /// </summary>
        private readonly HashSet<(EntityType, int)> _removedEntities
            = new HashSet<(EntityType, int)>();

        /// <summary>
        /// New entities created during this session.
        /// </summary>
        private readonly Dictionary<EntityType, List<IDEntity>> _newEntities
            = new Dictionary<EntityType, List<IDEntity>>();

        /// <summary>
        /// Returns true if there are any changes recorded.
        /// </summary>
        public bool HasChanges => _entityChanges.Any(e => e.Value.HasChanges)
                                  || _removedEntities.Count > 0
                                  || _newEntities.Any(e => e.Value.Count > 0);

        /// <summary>
        /// Gets or creates an EntityChanges for the specified entity.
        /// </summary>
        /// <param name="entityType">The type of entity.</param>
        /// <param name="entityId">The ID of the entity.</param>
        /// <param name="isVanilla">True if modifying a vanilla entity.</param>
        public EntityChanges GetOrCreateChanges(EntityType entityType, int entityId, bool isVanilla)
        {
            var key = (entityType, entityId);
            if (!_entityChanges.TryGetValue(key, out var changes))
            {
                changes = new EntityChanges
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    IsVanillaOverride = isVanilla
                };
                _entityChanges[key] = changes;
            }
            return changes;
        }

        /// <summary>
        /// Gets existing EntityChanges for an entity, if any.
        /// </summary>
        public bool TryGetChanges(EntityType entityType, int entityId, out EntityChanges changes)
        {
            return _entityChanges.TryGetValue((entityType, entityId), out changes);
        }

        /// <summary>
        /// Records a property change for an entity.
        /// </summary>
        public void RecordPropertyChange(IDEntity entity, Property property)
        {
            bool isVanilla = entity.IsVanilla || (LoadedMod != null && !IsInLoadedMod(entity));
            var changes = GetOrCreateChanges(GetEntityType(entity), entity.ID, isVanilla);
            changes.SetProperty(property);
        }

        /// <summary>
        /// Records a property removal for a mod entity.
        /// </summary>
        /// <returns>True if removal was recorded, false if entity is vanilla.</returns>
        public bool RecordPropertyRemoval(IDEntity entity, Command command)
        {
            bool isVanilla = entity.IsVanilla || (LoadedMod != null && !IsInLoadedMod(entity));
            if (isVanilla)
            {
                return false; // Cannot remove vanilla properties
            }

            var changes = GetOrCreateChanges(GetEntityType(entity), entity.ID, isVanilla);
            return changes.RemoveProperty(command);
        }

        /// <summary>
        /// Marks an entity as removed from the loaded mod.
        /// </summary>
        /// <returns>True if removal was recorded, false if entity is vanilla or no mod loaded.</returns>
        public bool RemoveEntity(IDEntity entity)
        {
            if (entity.IsVanilla || LoadedMod == null)
            {
                return false; // Cannot remove vanilla entities
            }

            var entityType = GetEntityType(entity);
            if (!IsInLoadedMod(entity))
            {
                return false; // Entity is not in loaded mod
            }

            _removedEntities.Add((entityType, entity.ID));
            // Clear any pending changes for this entity
            _entityChanges.Remove((entityType, entity.ID));
            return true;
        }

        /// <summary>
        /// Restores a previously removed entity.
        /// </summary>
        public bool RestoreEntity(EntityType entityType, int entityId)
        {
            return _removedEntities.Remove((entityType, entityId));
        }

        /// <summary>
        /// Adds a new entity created during this session.
        /// </summary>
        public void AddNewEntity(IDEntity entity)
        {
            var entityType = GetEntityType(entity);
            if (!_newEntities.TryGetValue(entityType, out var list))
            {
                list = new List<IDEntity>();
                _newEntities[entityType] = list;
            }
            list.Add(entity);
        }

        /// <summary>
        /// Removes a new entity that was created during this session.
        /// </summary>
        public bool RemoveNewEntity(IDEntity entity)
        {
            var entityType = GetEntityType(entity);
            if (_newEntities.TryGetValue(entityType, out var list))
            {
                return list.Remove(entity);
            }
            return false;
        }

        /// <summary>
        /// Checks if an entity is in the loaded mod.
        /// </summary>
        private bool IsInLoadedMod(IDEntity entity)
        {
            if (LoadedMod == null) return false;
            var entityType = GetEntityType(entity);
            return LoadedMod.Database.TryGetValue(entityType, out var set)
                   && set.TryGetValue(entity.ID, out _);
        }

        /// <summary>
        /// Checks if an entity has been removed.
        /// </summary>
        public bool IsEntityRemoved(EntityType entityType, int entityId)
        {
            return _removedEntities.Contains((entityType, entityId));
        }

        /// <summary>
        /// Gets all entity changes.
        /// </summary>
        public IEnumerable<EntityChanges> GetAllChanges()
        {
            return _entityChanges.Values.Where(c => c.HasChanges);
        }

        /// <summary>
        /// Gets all removed entity keys.
        /// </summary>
        public IEnumerable<(EntityType, int)> GetRemovedEntities()
        {
            return _removedEntities;
        }

        /// <summary>
        /// Gets all new entities of a specific type.
        /// </summary>
        public IEnumerable<IDEntity> GetNewEntities(EntityType entityType)
        {
            if (_newEntities.TryGetValue(entityType, out var list))
            {
                return list;
            }
            return Enumerable.Empty<IDEntity>();
        }

        /// <summary>
        /// Gets all new entities.
        /// </summary>
        public IEnumerable<IDEntity> GetAllNewEntities()
        {
            return _newEntities.Values.SelectMany(list => list);
        }

        /// <summary>
        /// Clears all changes, resetting to initial state.
        /// </summary>
        public void Clear()
        {
            _entityChanges.Clear();
            _removedEntities.Clear();
            _newEntities.Clear();
        }

        /// <summary>
        /// Reverts all changes for a specific entity.
        /// </summary>
        public void RevertEntity(EntityType entityType, int entityId)
        {
            _entityChanges.Remove((entityType, entityId));
            _removedEntities.Remove((entityType, entityId));
        }

        /// <summary>
        /// Reverts a single property change for an entity.
        /// </summary>
        public void RevertPropertyChange(IDEntity entity, Command command)
        {
            var entityType = GetEntityType(entity);
            if (_entityChanges.TryGetValue((entityType, entity.ID), out var changes))
            {
                changes.RevertProperty(command);
            }
        }

        /// <summary>
        /// Checks if a property has been changed in this session.
        /// </summary>
        public bool IsPropertyChanged(IDEntity entity, Command command)
        {
            var entityType = GetEntityType(entity);
            if (_entityChanges.TryGetValue((entityType, entity.ID), out var changes))
            {
                return changes.ChangedProperties.ContainsKey(command) || changes.RemovedProperties.Contains(command);
            }
            return false;
        }

        /// <summary>
        /// Gets the EntityType for an entity.
        /// </summary>
        private static EntityType GetEntityType(IDEntity entity)
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
                _ => throw new ArgumentException($"Unknown entity type: {entity.GetType()}")
            };
        }
    }
}
