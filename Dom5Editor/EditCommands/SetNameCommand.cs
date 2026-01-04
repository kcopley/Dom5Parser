using System;

namespace Dom5Editor.EditCommands
{
    /// <summary>
    /// Command for setting a name value using getter/setter delegates.
    /// Used for entity names and other delegate-based properties.
    /// </summary>
    public class SetNameCommand : IEditCommand
    {
        private readonly Func<string> _getter;
        private readonly Action<string> _setter;
        private readonly string _newValue;
        private readonly string _oldValue;
        private readonly string _propertyName;

        public string Description => $"Set {_propertyName}";

        /// <summary>
        /// Creates a command to set a name value.
        /// </summary>
        /// <param name="getter">Function to get the current value.</param>
        /// <param name="setter">Action to set the new value.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="propertyName">Name of the property for the description.</param>
        public SetNameCommand(Func<string> getter, Action<string> setter, string newValue, string propertyName = "Name")
        {
            _getter = getter;
            _setter = setter;
            _newValue = newValue;
            _oldValue = getter();
            _propertyName = propertyName;
        }

        public void Execute()
        {
            _setter(_newValue);
        }

        public void Undo()
        {
            _setter(_oldValue);
        }
    }
}
