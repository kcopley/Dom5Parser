using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Edit
{
    /// <summary>
    /// Tracks changes made to a single entity during an editing session.
    /// Used by ChangesMod to record deltas from vanilla or loaded mod state.
    /// </summary>
    public class EntityChanges
    {
        /// <summary>
        /// The ID of the entity being modified.
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// The type of entity (Monster, Weapon, etc.).
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// True if this is an override of a vanilla entity.
        /// Vanilla entities cannot have properties removed, only added/changed.
        /// </summary>
        public bool IsVanillaOverride { get; set; }

        /// <summary>
        /// Properties that have been changed or added.
        /// Key is the Command, Value is the new property value.
        /// </summary>
        public Dictionary<Command, Property> ChangedProperties { get; } = new Dictionary<Command, Property>();

        /// <summary>
        /// Properties that have been removed (only valid for mod entities, not vanilla).
        /// </summary>
        public HashSet<Command> RemovedProperties { get; } = new HashSet<Command>();

        /// <summary>
        /// Adds or updates a changed property.
        /// </summary>
        public void SetProperty(Property property)
        {
            ChangedProperties[property.Command] = property;
            // If we're setting a property, it's no longer removed
            RemovedProperties.Remove(property.Command);
        }

        /// <summary>
        /// Marks a property as removed. Only valid for mod entities.
        /// </summary>
        /// <returns>True if the removal was recorded, false if this is a vanilla override.</returns>
        public bool RemoveProperty(Command command)
        {
            if (IsVanillaOverride)
            {
                // Cannot remove properties from vanilla entities
                return false;
            }

            RemovedProperties.Add(command);
            // If we're removing, clear any pending change
            ChangedProperties.Remove(command);
            return true;
        }

        /// <summary>
        /// Clears a specific property change, reverting to original state.
        /// </summary>
        public void RevertProperty(Command command)
        {
            ChangedProperties.Remove(command);
            RemovedProperties.Remove(command);
        }

        /// <summary>
        /// Returns true if there are any changes recorded.
        /// </summary>
        public bool HasChanges => ChangedProperties.Count > 0 || RemovedProperties.Count > 0;

        /// <summary>
        /// Clears all changes for this entity.
        /// </summary>
        public void Clear()
        {
            ChangedProperties.Clear();
            RemovedProperties.Clear();
        }
    }
}
