using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dom5Editor.VMs
{
    public class IntPropertyViewModel : PropertyViewModel
    {
        public IntPropertyViewModel(IDEntity e, Command c) : base(e, c) { }
        public IntPropertyViewModel(string label, IDEntity e, Command c) : base(label, e, c) { }
        public IntPropertyViewModel(MonsterViewModel monster, string label, IDEntity e, Command c) : base(label, e, c)
        {
        }

        public SolidColorBrush BackgroundColor
        {
            get
            {
                var returned = Source.TryGet(_command, out IntProperty ip);
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
                var returned = Source.TryGet(_command, out IntProperty ip);
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
                    Source.Set<IntProperty>(_command, i => i.Value = ret);
                    OnPropertyChanged("BackgroundColor");
                    OnPropertyChanged("Value");
                }
            }
        }
    }
}
