using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.Commands
{
    /// <summary>
    /// Command for removing a property from an entity.
    /// Used for multi-value properties like weapons and armor on monsters.
    /// </summary>
    public class RemovePropertyCommand : IEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Property _property;

        public string Description { get; }

        /// <summary>
        /// Creates a command to remove a property from an entity.
        /// </summary>
        /// <param name="entity">The entity to remove the property from.</param>
        /// <param name="property">The property to remove.</param>
        /// <param name="description">Optional description for undo/redo menu.</param>
        public RemovePropertyCommand(IDEntity entity, Property property, string description = null)
        {
            _entity = entity;
            _property = property;
            Description = description ?? $"Remove {property.Command}";
        }

        public void Execute()
        {
            _entity.RemoveProperty(_property);
        }

        public void Undo()
        {
            // Re-add the property (AddProperty will sort it into the correct position)
            _entity.AddProperty(_property);
        }
    }
}
