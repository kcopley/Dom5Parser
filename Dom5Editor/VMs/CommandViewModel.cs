using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dom5Editor.VMs
{
    public class CommandViewModel : PropertyViewModel
    {
        public CommandViewModel(IDEntity e, Command c) : base(e, c) { }
        public CommandViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(_command, out CommandProperty ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Command.ToString();
                    case ReturnType.TRUE:
                        return ip.Command.ToString();
                }
                return "";
            }
            set
            {
                Source.SetCommand<CommandProperty>(_command);
                OnPropertyChanged(_command.ToString());
            }
        }
    }
}
