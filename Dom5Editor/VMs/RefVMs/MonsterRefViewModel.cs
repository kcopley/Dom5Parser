using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class MonsterRefViewModel : PropertyViewModel
    {
        public MonsterRefViewModel(ModViewModel parent, ViewModelBase owner, IDEntity e, MonsterOrMontagRef p) : base(e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
            History = parent?.History;
        }
        public MonsterRefViewModel(ModViewModel parent, ViewModelBase owner, string label, IDEntity e, MonsterOrMontagRef p) : base(label, e, p.Command)
        {
            _parentMod = parent;
            _owner = owner;
            _property = p;
            History = parent?.History;
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
                if (value != null)
                {
                    if (History != null)
                    {
                        var cmd = new SetReferenceCommand(_property, value.Monster, "Monster");
                        History.Execute(cmd);
                    }
                    else
                    {
                        _property.TrySetEntity(value.Monster);
                    }
                    _parentMod.OnPropertyChanged("");
                    _owner.OnPropertyChanged("");
                    OnPropertyChanged(nameof(SelectedID));
                }
            }
        }
    }
}
