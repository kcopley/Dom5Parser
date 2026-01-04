using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Editor.Commands;

namespace Dom5Editor.VMs
{
    public abstract class PropertyViewModel : ViewModelBase
    {
        public IDEntity Source { get; }
        public Command Command { get; private set; }
        public string Label { get; }

        /// <summary>
        /// Optional command history for undo/redo support.
        /// When set, property changes are routed through commands.
        /// </summary>
        public CommandHistory History { get; set; }

        public PropertyViewModel(IDEntity e, Command c)
        {
            this.Label = c.ToString();
            this.Source = e;
            this.Command = c;
        }

        public PropertyViewModel(string label, IDEntity e, Command c)
        {
            this.Label = label;
            this.Source = e;
            this.Command = c;
        }

        public PropertyViewModel(string label, IDEntity e, Command c, CommandHistory history)
        {
            this.Label = label;
            this.Source = e;
            this.Command = c;
            this.History = history;
        }

        public abstract string Value { get; set; }
    }
}
