using System.Windows.Media;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class IntPropertyViewModel : PropertyViewModel
    {
        public IntPropertyViewModel(IDEntity e, Command c) : base(e, c) { }
        public IntPropertyViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public IntPropertyViewModel(MonsterViewModel monster, string label, IDEntity e, Command c) : base(label, e, c)
        {
            // Get History from the parent ModViewModel
            History = monster?.Parent?.History;
        }

        public IntPropertyViewModel(string label, IDEntity e, Command c, CommandHistory history)
            : base(label, e, c, history) { }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                var returned = Source.TryGet(Command, out IntProperty ip);
                switch (returned)
                {
                    case ReturnType.FALSE:
                        return new SolidColorBrush(Colors.Red);
                    case ReturnType.COPIED:
                        return new SolidColorBrush(Colors.LightGray);
                    case ReturnType.TRUE:
                        return new SolidColorBrush(Colors.White);
                }
                return new SolidColorBrush(Colors.Red);
            }
        }

        public override string Value
        {
            get
            {
                var returned = Source.TryGet(Command, out IntProperty ip);
                switch (returned)
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        //set to greyed out?
                        return ip.Value.ToString();
                    case ReturnType.TRUE:
                        return ip.Value.ToString();
                }
                return "";
            }
            set
            {
                if (int.TryParse(value, out int ret))
                {
                    if (History != null)
                    {
                        // Use command pattern for undo/redo support
                        var cmd = new SetIntPropertyCommand(Source, Command, ret);
                        History.Execute(cmd);
                    }
                    else
                    {
                        // Fallback to direct modification (for backwards compatibility)
                        Source.Set<IntProperty>(Command, i => i.Value = ret);
                    }
                    OnPropertyChanged("BackgroundColor");
                    OnPropertyChanged("Value");
                }
            }
        }
    }
}
