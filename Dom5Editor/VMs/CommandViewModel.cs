using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class CommandViewModel : PropertyViewModel
    {
        public CommandViewModel(IDEntity e, Command c) : base(e, c) { }
        public CommandViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public CommandViewModel(string label, IDEntity e, Command c, CommandHistory history)
            : base(label, e, c, history) { }

        /// <summary>
        /// Gets or sets whether this command flag is enabled.
        /// For binding to checkboxes.
        /// </summary>
        public bool IsChecked
        {
            get
            {
                var result = Source.TryGet(Command, out CommandProperty _);
                return result == ReturnType.TRUE || result == ReturnType.COPIED;
            }
            set
            {
                if (History != null)
                {
                    var cmd = new SetCommandPropertyCommand(Source, Command, value);
                    History.Execute(cmd);
                }
                else
                {
                    if (value)
                        Source.SetCommand<CommandProperty>(Command);
                    else
                        Source.Remove<CommandProperty>(Command);
                }
                OnPropertyChanged(nameof(IsChecked));
                OnPropertyChanged(nameof(Value));
            }
        }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out CommandProperty ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        return ip.Command.ToString();
                    case ReturnType.TRUE:
                        return ip.Command.ToString();
                }
                return "";
            }
            set
            {
                // Toggle the flag
                IsChecked = !IsChecked;
            }
        }
    }
}
