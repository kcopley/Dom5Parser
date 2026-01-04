using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for setting an IntIntProperty (two integer values) on an entity.
    /// Supports undo by capturing the previous state before modification.
    /// </summary>
    public class SetIntIntPropertyCommand : IPropertyEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Command _command;
        private readonly int _newValue1;
        private readonly int _newValue2;
        private readonly int? _oldValue1;
        private readonly int? _oldValue2;
        private readonly bool _propertyExisted;
        private readonly bool _isValue1Change;

        public string Description { get; }

        // IPropertyEditCommand implementation
        public IDEntity Entity => _entity;
        public Command PropertyCommand => _command;
        public bool IsRemoval => false;

        public Property GetResultingProperty()
        {
            _entity.TryGet<IntIntProperty>(_command, out var prop, checkCopy: false);
            return prop;
        }

        /// <summary>
        /// Creates a command to set both values of an IntIntProperty.
        /// </summary>
        public SetIntIntPropertyCommand(IDEntity entity, Command command, int newValue1, int newValue2)
        {
            _entity = entity;
            _command = command;
            _newValue1 = newValue1;
            _newValue2 = newValue2;
            _isValue1Change = true; // Setting both

            Description = $"Set {_command} to {_newValue1}, {_newValue2}";

            // Capture current state for undo
            var result = _entity.TryGet<IntIntProperty>(command, out var existingProp, checkCopy: false);
            _propertyExisted = result == ReturnType.TRUE;
            if (_propertyExisted)
            {
                _oldValue1 = existingProp.Value1;
                _oldValue2 = existingProp.Value2;
            }
        }

        /// <summary>
        /// Creates a command to set just Value1 of an IntIntProperty.
        /// </summary>
        public static SetIntIntPropertyCommand ForValue1(IDEntity entity, Command command, int newValue1)
        {
            return new SetIntIntPropertyCommand(entity, command, newValue1, isValue1: true);
        }

        /// <summary>
        /// Creates a command to set just Value2 of an IntIntProperty.
        /// </summary>
        public static SetIntIntPropertyCommand ForValue2(IDEntity entity, Command command, int newValue2)
        {
            return new SetIntIntPropertyCommand(entity, command, newValue2, isValue1: false);
        }

        private SetIntIntPropertyCommand(IDEntity entity, Command command, int newValue, bool isValue1)
        {
            _entity = entity;
            _command = command;
            _isValue1Change = isValue1;

            // Capture current state for undo
            var result = _entity.TryGet<IntIntProperty>(command, out var existingProp, checkCopy: false);
            _propertyExisted = result == ReturnType.TRUE;

            if (_propertyExisted)
            {
                _oldValue1 = existingProp.Value1;
                _oldValue2 = existingProp.Value2;
            }

            if (isValue1)
            {
                _newValue1 = newValue;
                _newValue2 = _oldValue2 ?? 0;
                Description = $"Set {_command} value1 to {_newValue1}";
            }
            else
            {
                _newValue1 = _oldValue1 ?? 0;
                _newValue2 = newValue;
                Description = $"Set {_command} value2 to {_newValue2}";
            }
        }

        public void Execute()
        {
            _entity.Set<IntIntProperty>(_command, p =>
            {
                p.Value1 = _newValue1;
                p.Value2 = _newValue2;
            });
        }

        public void Undo()
        {
            if (_propertyExisted && _oldValue1.HasValue && _oldValue2.HasValue)
            {
                // Restore the old values
                _entity.Set<IntIntProperty>(_command, p =>
                {
                    p.Value1 = _oldValue1.Value;
                    p.Value2 = _oldValue2.Value;
                });
            }
            else
            {
                // Property didn't exist before, remove it
                _entity.Remove<IntIntProperty>(_command);
            }
        }
    }
}
