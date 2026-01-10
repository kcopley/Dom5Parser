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
        /// Original property values before any session edits.
        /// Used for identity checking - if a value is edited back to its original,
        /// the change is automatically removed from tracking.
        /// </summary>
        private readonly Dictionary<Command, Property> _originalValues = new Dictionary<Command, Property>();

        /// <summary>
        /// Original "property existed" state for commands that didn't have a value before session.
        /// Used to detect when a property is removed back to its original non-existent state.
        /// </summary>
        private readonly HashSet<Command> _originallyMissing = new HashSet<Command>();

        /// <summary>
        /// Adds or updates a changed property (legacy method without identity check).
        /// </summary>
        public void SetProperty(Property property)
        {
            ChangedProperties[property.Command] = property;
            // If we're setting a property, it's no longer removed
            RemovedProperties.Remove(property.Command);
        }

        /// <summary>
        /// Adds or updates a changed property with identity checking.
        /// If the new value matches the original (pre-session) value, the change is removed.
        /// </summary>
        /// <param name="property">The new property value.</param>
        /// <param name="originalBeforeCommand">The property value before the current edit command.
        /// On the first edit of a property, this is the session original.</param>
        /// <returns>True if the property was recorded as changed, false if it matched the original and was reverted.</returns>
        public bool SetPropertyWithOriginal(Property property, Property originalBeforeCommand)
        {
            var command = property.Command;

            // First time we touch this command in this session, record the original
            if (!_originalValues.ContainsKey(command) && !_originallyMissing.Contains(command))
            {
                if (originalBeforeCommand != null)
                {
                    _originalValues[command] = originalBeforeCommand;
                }
                else
                {
                    _originallyMissing.Add(command);
                }
            }

            // Get the session original (what was there before ANY session edits)
            _originalValues.TryGetValue(command, out var sessionOriginal);

            // Compare new value to session original
            if (ArePropertiesEqual(property, sessionOriginal))
            {
                // Back to original - clear the change
                ChangedProperties.Remove(command);
                // Keep _originalValues entry so we remember the original for future edits
                return false;
            }

            ChangedProperties[command] = property;
            RemovedProperties.Remove(command);
            return true;
        }

        /// <summary>
        /// Compares two properties for value equality.
        /// </summary>
        private static bool ArePropertiesEqual(Property a, Property b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Command != b.Command) return false;

            // Compare by property type
            return (a, b) switch
            {
                (IntProperty ia, IntProperty ib) => ia.Value == ib.Value,
                (IntIntProperty iia, IntIntProperty iib) => iia.Value1 == iib.Value1 && iia.Value2 == iib.Value2,
                (StringProperty sa, StringProperty sb) => sa.Value == sb.Value,
                (CommandProperty, CommandProperty) => true, // Both exist = equal
                // For other property types, compare by reference or consider them different
                _ => ReferenceEquals(a, b)
            };
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
