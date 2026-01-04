using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for setting a string property value on an entity.
    /// Supports undo by capturing the previous state before modification.
    /// </summary>
    public class SetStringPropertyCommand : IPropertyEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Command _command;
        private readonly string _newValue;
        private readonly string _oldValue;
        private readonly bool _propertyExisted;

        public string Description => $"Set {_command}";

        // IPropertyEditCommand implementation
        public IDEntity Entity => _entity;
        public Command PropertyCommand => _command;
        public bool IsRemoval => false;

        public Property GetResultingProperty()
        {
            _entity.TryGet<StringProperty>(_command, out var prop, checkCopy: false);
            return prop;
        }

        /// <summary>
        /// Creates a command to set a string property value.
        /// </summary>
        /// <param name="entity">The entity to modify.</param>
        /// <param name="command">The property command (e.g., Command.NAME).</param>
        /// <param name="newValue">The new value to set.</param>
        public SetStringPropertyCommand(IDEntity entity, Command command, string newValue)
        {
            _entity = entity;
            _command = command;
            _newValue = newValue;

            // Capture current state for undo
            var result = _entity.TryGet<StringProperty>(command, out var existingProp, checkCopy: false);
            _propertyExisted = result == ReturnType.TRUE;
            if (_propertyExisted)
            {
                _oldValue = existingProp.Value;
            }
        }

        public void Execute()
        {
            _entity.Set<StringProperty>(_command, p => p.Value = _newValue);
        }

        public void Undo()
        {
            if (_propertyExisted && _oldValue != null)
            {
                // Restore the old value
                _entity.Set<StringProperty>(_command, p => p.Value = _oldValue);
            }
            else
            {
                // Property didn't exist before, remove it
                _entity.Remove<StringProperty>(_command);
            }
        }
    }
}
