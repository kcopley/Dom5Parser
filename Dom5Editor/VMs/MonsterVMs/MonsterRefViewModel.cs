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
    public class MonsterRefViewModel : PropertyViewModel
    {
        public MonsterRefViewModel(ModViewModel parent, ViewModelBase owner, IDEntity e, MonsterOrMontagRef p) : base(e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
        }
        public MonsterRefViewModel(ModViewModel parent, ViewModelBase owner, string label, IDEntity e, MonsterOrMontagRef p) : base(label, e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
        }

        internal ModViewModel _parentMod;
        internal ViewModelBase _owner;
        internal MonsterOrMontagRef _property;

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

        public List<MonsterViewModel> IDs
        {
            get { return _parentMod.Monsters; }
        }

        public MonsterViewModel SelectedID
        {
            get
            {
                return _parentMod.Monsters.Find(x =>
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
                _property.TrySetEntity(value.Monster);
                // Source.Set<StringOrIDRef>(_command, id => id.Entity = value.Monster);
                _parentMod.OnPropertyChanged("");
                _owner.OnPropertyChanged("");
            }
        }
    }
}
