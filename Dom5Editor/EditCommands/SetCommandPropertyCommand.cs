using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for toggling a CommandProperty (boolean flag) on an entity.
    /// CommandProperties are flags that are either present or absent.
    /// </summary>
    public class SetCommandPropertyCommand : IPropertyEditCommand
    {
        private readonly IDEntity _entity;
        private readonly Command _command;
        private readonly bool _setPresent;
        private readonly bool _wasPresent;

        public string Description { get; }

        // IPropertyEditCommand implementation
        public IDEntity Entity => _entity;
        public Command PropertyCommand => _command;
        public bool IsRemoval => !_setPresent;

        public Property GetResultingProperty()
        {
            if (!_setPresent) return null;
            _entity.TryGet<CommandProperty>(_command, out var prop, checkCopy: false);
            return prop;
        }

        public Property GetOriginalProperty()
        {
            // For CommandProperty, the original is a flag that was present
            // Return a CommandProperty if it was present, null if it wasn't
            if (_wasPresent)
            {
                return CommandProperty.Create(_command, _entity);
            }
            return null;
        }

        /// <summary>
        /// Creates a command to set or clear a CommandProperty flag.
        /// </summary>
        /// <param name="entity">The entity to modify.</param>
        /// <param name="command">The command flag (e.g., Command.FLYING).</param>
        /// <param name="setPresent">True to add the flag, false to remove it.</param>
        public SetCommandPropertyCommand(IDEntity entity, Command command, bool setPresent)
        {
            _entity = entity;
            _command = command;
            _setPresent = setPresent;

            // Capture current state for undo
            var result = _entity.TryGet<CommandProperty>(command, out _, checkCopy: false);
            _wasPresent = result == ReturnType.TRUE;

            Description = setPresent ? $"Enable {_command}" : $"Disable {_command}";
        }

        public void Execute()
        {
            if (_setPresent)
            {
                // Add the flag if not present
                if (!_wasPresent)
                {
                    _entity.Set<CommandProperty>(_command, _ => { });
                }
            }
            else
            {
                // Remove the flag if present
                _entity.Remove<CommandProperty>(_command);
            }
        }

        public void Undo()
        {
            if (_wasPresent)
            {
                // Restore the flag
                _entity.Set<CommandProperty>(_command, _ => { });
            }
            else
            {
                // Remove the flag
                _entity.Remove<CommandProperty>(_command);
            }
        }
    }
}
