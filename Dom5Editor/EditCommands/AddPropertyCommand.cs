using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for adding a property to an entity.
    /// Used for multi-value properties like weapons and armor on monsters.
    /// </summary>
    public class AddPropertyCommand : IPropertyEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Property _property;

        public string Description { get; }

        // IPropertyEditCommand implementation
        public IDEntity Entity => _entity;
        public Command PropertyCommand => _property.Command;
        public bool IsRemoval => false;

        public Property GetResultingProperty() => _property;

        /// <summary>
        /// Creates a command to add a property to an entity.
        /// </summary>
        /// <param name="entity">The entity to add the property to.</param>
        /// <param name="property">The property to add.</param>
        /// <param name="description">Optional description for undo/redo menu.</param>
        public AddPropertyCommand(IDEntity entity, Property property, string description = null)
        {
            _entity = entity;
            _property = property;
            Description = description ?? $"Add {property.Command}";
        }

        public void Execute()
        {
            _entity.AddProperty(_property);
        }

        public void Undo()
        {
            _entity.RemoveProperty(_property);
        }
    }
}
