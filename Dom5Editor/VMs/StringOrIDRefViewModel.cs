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
    public class StringOrIDRefViewModel : PropertyViewModel
    {
        public StringOrIDRefViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public StringOrIDRefViewModel(IDEntity e, Command c) : base(e, c) { }
        

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out StringOrIDRef ip))
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Entity.Name;
                    case ReturnType.TRUE:
                        return ip.Entity.Name;
                }
                return null;
            }
            set
            {
                Source.Set<StringOrIDRef>(Command, i =>
                {
                    this.Source.ParentMod.TryGet(EntityType.WEAPON, int.Parse(value), "", out var e);
                    i.Entity = e;
                });
                OnPropertyChanged(Command.ToString());
            }
        }
    }
}
