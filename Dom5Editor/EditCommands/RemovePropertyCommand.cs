using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for removing a property from an entity.
    /// Used for multi-value properties like weapons and armor on monsters.
    /// </summary>
    public class RemovePropertyCommand : IPropertyEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Property _property;

        public string Description { get; }

        // IPropertyEditCommand implementation
        public IDEntity Entity => _entity;
        public Command PropertyCommand => _property.Command;
        public bool IsRemoval => true;

        public Property GetResultingProperty() => null;

        public Property GetOriginalProperty()
        {
            // The property being removed IS the original
            return _property;
        }

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
