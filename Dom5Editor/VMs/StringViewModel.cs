using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class StringViewModel : PropertyViewModel
    {
        public StringViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public StringViewModel(IDEntity e, Command c) : base(e, c) { }
        public StringViewModel(string label, IDEntity e, Command c, CommandHistory history)
            : base(label, e, c, history) { }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out StringProperty ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Value;
                    case ReturnType.TRUE:
                        return ip.Value;
                }
                return "";
            }
            set
            {
                if (History != null)
                {
                    // Use command pattern for undo/redo support
                    var cmd = new SetStringPropertyCommand(Source, Command, value);
                    History.Execute(cmd);
                }
                else
                {
                    // Fallback to direct modification
                    Source.Set<StringProperty>(Command, i => i.Value = value);
                }
                OnPropertyChanged(Command.ToString());
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}
