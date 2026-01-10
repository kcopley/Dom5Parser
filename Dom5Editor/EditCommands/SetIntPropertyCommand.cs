using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for setting an integer property value on an entity.
    /// Supports undo by capturing the previous state before modification.
    /// </summary>
    public class SetIntPropertyCommand : IPropertyEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Command _command;
        private readonly int _newValue;
        private readonly int? _oldValue;
        private readonly bool _propertyExisted;
        private readonly IntProperty _originalProperty;

        public string Description => $"Set {_command} to {_newValue}";

        // IPropertyEditCommand implementation
        public IDEntity Entity => _entity;
        public Command PropertyCommand => _command;
        public bool IsRemoval => false;

        public Property GetResultingProperty()
        {
            _entity.TryGet<IntProperty>(_command, out var prop, checkCopy: false);
            return prop;
        }

        public Property GetOriginalProperty()
        {
            return _originalProperty;
        }

        /// <summary>
        /// Creates a command to set an integer property value.
        /// </summary>
        /// <param name="entity">The entity to modify.</param>
        /// <param name="command">The property command (e.g., Command.HP).</param>
        /// <param name="newValue">The new value to set.</param>
        public SetIntPropertyCommand(IDEntity entity, Command command, int newValue)
        {
            _entity = entity;
            _command = command;
            _newValue = newValue;

            // Capture current state for undo and identity checking
            var result = _entity.TryGet<IntProperty>(command, out var existingProp, checkCopy: false);
            _propertyExisted = result == ReturnType.TRUE;
            if (_propertyExisted)
            {
                _oldValue = existingProp.Value;
                // Create a snapshot of the original property for identity checking
                _originalProperty = IntProperty.Create(command, entity, existingProp.Value);
            }
        }

        public void Execute()
        {
            _entity.Set<IntProperty>(_command, p => p.Value = _newValue);
        }

        public void Undo()
        {
            if (_propertyExisted && _oldValue.HasValue)
            {
                // Restore the old value
                _entity.Set<IntProperty>(_command, p => p.Value = _oldValue.Value);
            }
            else
            {
                // Property didn't exist before, remove it
                _entity.Remove<IntProperty>(_command);
            }
        }
    }
}
