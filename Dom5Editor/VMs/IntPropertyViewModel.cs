using System.Windows.Media;
using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.EditCommands;

namespace Dom5Editor.VMs
{
    public class IntIntPropertyViewModel : PropertyViewModel
    {
        public IntIntPropertyViewModel(IDEntity e, Command c) : base(e, c) { }
        public IntIntPropertyViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public IntIntPropertyViewModel(string label, IDEntity e, Command c, CommandHistory history)
            : base(label, e, c, history) { }
        public IntIntPropertyViewModel(MonsterViewModel monster, string label, IDEntity e, Command c) : base(label, e, c)
        {
            History = monster?.Parent?.History;
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                var returned = Source.TryGet(Command, out IntIntProperty ip);
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

        public string Value1
        {
            get
            {
                var returned = Source.TryGet(Command, out IntIntProperty ip);
                switch (returned)
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        return ip.Value1.ToString();
                    case ReturnType.TRUE:
                        return ip.Value1.ToString();
                }
                return "";
            }
            set
            {
                if (int.TryParse(value, out int ret))
                {
                    if (History != null)
                    {
                        var cmd = SetIntIntPropertyCommand.ForValue1(Source, Command, ret);
                        History.Execute(cmd);
                    }
                    else
                    {
                        Source.Set<IntIntProperty>(Command, i => i.Value1 = ret);
                    }
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(Value1));
                }
            }
        }

        public string Value2
        {
            get
            {
                var returned = Source.TryGet(Command, out IntIntProperty ip);
                switch (returned)
                {
                    case ReturnType.FALSE:
                        break;
                    case ReturnType.COPIED:
                        return ip.Value2.ToString();
                    case ReturnType.TRUE:
                        return ip.Value2.ToString();
                }
                return "";
            }
            set
            {
                if (int.TryParse(value, out int ret))
                {
                    if (History != null)
                    {
                        var cmd = SetIntIntPropertyCommand.ForValue2(Source, Command, ret);
                        History.Execute(cmd);
                    }
                    else
                    {
                        Source.Set<IntIntProperty>(Command, i => i.Value2 = ret);
                    }
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(Value2));
                }
            }
        }

        public override string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
