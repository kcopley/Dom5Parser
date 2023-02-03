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
    public class IntPropertyViewModel : ViewModelBase
    {
        private string _label;
        public string Label
        {
            get
            {
                return _label;
            }
        }

        private bool _hasValue;
        private int _value;
        public string Value
        {
            get
            {
                switch (Source.TryGet<IntProperty>(_command, out IntProperty ip))
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
                    switch (Source.TryGet<IntProperty>(_command, out IntProperty ip))
                    {
                        case ReturnType.FALSE:
                            //create the property
                            break;
                        case ReturnType.COPIED:
                            if (ip.Value == ret)
                            {
                                //remove property
                            }
                            else
                            {
                                //add property
                            }
                            break;
                        case ReturnType.TRUE:
                            var copy = Source.TryGetCopyValue<IntProperty>(_command, out IntProperty copyFrom);
                            if (copy == ReturnType.COPIED && ret == copyFrom.Value)
                            {
                                //remove the property
                            }
                            else
                            {
                                //adjust the property
                            }
                            break;
                    }
                }
            }
        }

        private IDEntity _source;
        public IDEntity Source { get { return _source; } }
        private Command _command;

        public IntPropertyViewModel(string label, IDEntity e, Command c)
        {
            this._label = label;
            this._source = e;
            this._command = c;
        }
    }
}
