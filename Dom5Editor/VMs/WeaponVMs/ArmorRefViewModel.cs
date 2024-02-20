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
    public class ArmorRefViewModel : PropertyViewModel
    {
        public ArmorRefViewModel(ModViewModel parent, ViewModelBase owner, IDEntity e, ArmorRef p) : base(e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
        }
        public ArmorRefViewModel(ModViewModel parent, ViewModelBase owner, string label, IDEntity e, ArmorRef p) : base(label, e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
        }

        internal ModViewModel _parentMod;
        internal ViewModelBase _owner;
        internal ArmorRef _property;

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
                        return ip.Command.ToString();
                    case ReturnType.TRUE:
                        return ip.Command.ToString();
                }
                return "";
            }
            set
            {
                Source.Set<StringOrIDRef>(Command, id => id.SetEntity(value));
                OnPropertyChanged(Command.ToString());
            }
        }

        public List<ArmorViewModel> IDs
        {
            get { return _parentMod.Armors; }
        }

        public ArmorViewModel SelectedID
        {
            get
            {
                return _parentMod.Armors.Find(x =>
                {
                    if (_property.TryGetEntity(out var e))
                    {
                        return x.ID == e.ID;
                    }
                    return false;
                });
            }
            set
            {
                if (value == null) return;
                _property.Entity = value.Armor;
                _parentMod.OnPropertyChanged("");
                _owner.OnPropertyChanged("");
            }
        }
    }
}
