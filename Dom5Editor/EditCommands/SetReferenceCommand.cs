using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for changing which entity a reference property points to.
    /// Works with StringOrIDRef and IDRef types that have an Entity property.
    /// </summary>
    public class SetReferenceCommand : IPropertyEditCommand
    {
        private readonly Reference _reference;
        private readonly IDEntity _newEntity;
        private readonly IDEntity _oldEntity;
        private readonly string _refTypeName;

        public string Description { get; }

        // IPropertyEditCommand implementation
        public IDEntity Entity => _reference.Parent;
        public Command PropertyCommand => _reference.Command;
        public bool IsRemoval => false;

        public Property GetResultingProperty() => _reference;

        /// <summary>
        /// Creates a command to change the entity a reference points to.
        /// </summary>
        /// <param name="reference">The reference property to modify (must be StringOrIDRef or IDRef).</param>
        /// <param name="newEntity">The new entity to reference.</param>
        /// <param name="refTypeName">Name of the reference type for description.</param>
        public SetReferenceCommand(Reference reference, IDEntity newEntity, string refTypeName = "Reference")
        {
            _reference = reference;
            _newEntity = newEntity;
            _refTypeName = refTypeName;

            // Capture old entity for undo
            _oldEntity = GetEntity();

            var newName = newEntity?.Name ?? newEntity?.ID.ToString() ?? "none";
            Description = $"Set {_refTypeName} to {newName}";
        }

        private IDEntity GetEntity()
        {
            if (_reference is StringOrIDRef stringRef)
                return stringRef.Entity;
            if (_reference is IDRef idRef)
                return idRef.Entity;
            if (_reference is MonsterOrMontagRef monRef && monRef.TryGetEntity(out var e))
                return e;
            return null;
        }

        private void SetEntity(IDEntity entity)
        {
            if (_reference is StringOrIDRef stringRef)
                stringRef.Entity = entity;
            else if (_reference is IDRef idRef)
                idRef.Entity = entity;
            else if (_reference is MonsterOrMontagRef monRef)
                monRef.TrySetEntity(entity);
        }

        public void Execute()
        {
            SetEntity(_newEntity);
        }

        public void Undo()
        {
            SetEntity(_oldEntity);
        }
    }
}
