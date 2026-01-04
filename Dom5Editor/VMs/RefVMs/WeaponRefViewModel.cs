using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class WeaponRefViewModel : PropertyViewModel
    {
        public WeaponRefViewModel(ModViewModel parent, ViewModelBase owner, IDEntity e, WeaponRef p) : base(e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
            History = parent?.History;
        }
        public WeaponRefViewModel(ModViewModel parent, ViewModelBase owner, string label, IDEntity e, WeaponRef p) : base(label, e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
            History = parent?.History;
        }

        internal ModViewModel _parentMod;
        internal ViewModelBase _owner;
        internal WeaponRef _property;

        public override string Value
        {
            get
            {
                switch (Source.TryGet(Command, out StringOrIDRef ip))
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
                Source.Set<StringOrIDRef>(Command, id => id.SetEntity(value));
                OnPropertyChanged(Command.ToString());
            }
        }

        public List<WeaponViewModel> IDs
        {
            get { return _parentMod.Weapons; }
        }

        public WeaponViewModel SelectedID
        {
            get
            {
                return _parentMod.Weapons.Find(x =>
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

                if (History != null)
                {
                    var cmd = new SetReferenceCommand(_property, value.Weapon, "Weapon");
                    History.Execute(cmd);
                }
                else
                {
                    _property.Entity = value.Weapon;
                }
                _parentMod.OnPropertyChanged("");
                _owner.OnPropertyChanged("");
                OnPropertyChanged(nameof(SelectedID));
            }
        }
    }
}
