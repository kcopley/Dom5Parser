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
    public class NameViewModel : PropertyViewModel
    {
        public NameViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public NameViewModel(IDEntity e, Command c) : base(e, c) { }

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out NameProperty ip))
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
                //set entity name here
                Source.Set<NameProperty>(Command, i => i.Value = value);
                OnPropertyChanged(Command.ToString());
            }
        }
    }
}
