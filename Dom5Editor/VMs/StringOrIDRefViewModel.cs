using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class StringOrIDRefViewModel : PropertyViewModel
    {
        public StringOrIDRefViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public StringOrIDRefViewModel(IDEntity e, Command c) : base(e, c) { }
        public StringOrIDRefViewModel(string label, IDEntity e, Command c, CommandHistory history)
            : base(label, e, c, history) { }

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
                        return ip.Entity?.Name ?? "";
                    case ReturnType.TRUE:
                        return ip.Entity?.Name ?? "";
                }
                return null;
            }
            set
            {
                if (History != null && Source.TryGet<StringOrIDRef>(Command, out var existingRef) == ReturnType.TRUE)
                {
                    // Try to find the entity
                    this.Source.ParentMod.TryGet(EntityType.WEAPON, int.Parse(value), "", out var newEntity);
                    if (newEntity != null)
                    {
                        var cmd = new SetReferenceCommand(existingRef, newEntity as IDEntity, "Reference");
                        History.Execute(cmd);
                    }
                }
                else
                {
                    Source.Set<StringOrIDRef>(Command, i =>
                    {
                        this.Source.ParentMod.TryGet(EntityType.WEAPON, int.Parse(value), "", out var e);
                        i.Entity = e;
                    });
                }
                OnPropertyChanged(Command.ToString());
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}
