using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class CopyStatsRefViewModel : PropertyViewModel
    {
        public CopyStatsRefViewModel(ModViewModel parent, ViewModelBase owner, IDEntity e, Command c) : base(e, c)
        {
            _parentMod = parent;
            _owner = owner;
            History = parent?.History;
        }
        public CopyStatsRefViewModel(ModViewModel parent, ViewModelBase owner, string label, IDEntity e, Command c) : base(label, e, c)
        {
            _parentMod = parent;
            _owner = owner;
            History = parent?.History;
        }

        internal ModViewModel _parentMod;
        internal ViewModelBase _owner;

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
                if (Source.TryGet<CopyStatsRef>(Command, out var rf) == ReturnType.TRUE)
                {
                    return _parentMod.Monsters.Find(x => x.ID == rf.ID);
                }
                return null;
            }
            set
            {
                if (value == null) return;

                if (History != null)
                {
                    // Get the reference property for command support
                    if (this.Command == Command.COPYSTATS && Source.TryGet<CopyStatsRef>(Command, out var copyRef) == ReturnType.TRUE)
                    {
                        var cmd = new SetReferenceCommand(copyRef, value.Monster, "CopyStats");
                        History.Execute(cmd);
                    }
                    else if (Source.TryGet<StringOrIDRef>(Command, out var idRef) == ReturnType.TRUE)
                    {
                        var cmd = new SetReferenceCommand(idRef, value.Monster, "Reference");
                        History.Execute(cmd);
                    }
                    else
                    {
                        // Fallback to direct set if property doesn't exist yet
                        if (this.Command == Command.COPYSTATS)
                        {
                            Source.Set<CopyStatsRef>(Command, id => id.Entity = value.Monster);
                        }
                        else
                        {
                            Source.Set<StringOrIDRef>(Command, id => id.Entity = value.Monster);
                        }
                    }
                }
                else
                {
                    if (this.Command == Command.COPYSTATS)
                    {
                        Source.Set<CopyStatsRef>(Command, id => id.Entity = value.Monster);
                    }
                    else
                    {
                        Source.Set<StringOrIDRef>(Command, id => id.Entity = value.Monster);
                    }
                }
                _parentMod.OnPropertyChanged("");
                _owner.OnPropertyChanged("");
                OnPropertyChanged(nameof(SelectedID));
            }
        }
    }
}
