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
    public class NameViewModel : PropertyViewModel
    {
        private readonly Func<string> _getter;
        private readonly Action<string> _setter;

        public NameViewModel(IDEntity e, Command c) : base(e, c)
        {
            _getter = () => e.Name;
            _setter = value => e.Name = value;
        }

        public NameViewModel(string label, IDEntity e, Command c) : base(label, e, c)
        {
            _getter = () => e.Name;
            _setter = value => e.Name = value;
        }

        public NameViewModel(IDEntity e, Command c, Func<string> getter, Action<string> setter) : base(e, c)
        {
            _getter = getter;
            _setter = setter;
        }

        public NameViewModel(string label, IDEntity e, Command c, Func<string> getter, Action<string> setter) : base(label, e, c)
        {
            _getter = getter;
            _setter = setter;
        }

        public override string Value
        {
            get => _getter();
            set
            {
                if (_getter() != value)
                {
                    _setter(value);
                    OnPropertyChanged(nameof(Value));
                }
            }
        }
    }
}
