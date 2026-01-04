using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class DescriptionViewModel : PropertyViewModel
    {
        public DescriptionViewModel(IDEntity e, Command c) : base(e, c) { }
        public DescriptionViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public DescriptionViewModel(string label, IDEntity e, Command c, CommandHistory history)
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
                    var cmd = new SetStringPropertyCommand(Source, Command, value);
                    History.Execute(cmd);
                }
                else
                {
                    Source.Set<StringProperty>(Command, i => i.Value = value);
                }
                OnPropertyChanged(Command.ToString());
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}
