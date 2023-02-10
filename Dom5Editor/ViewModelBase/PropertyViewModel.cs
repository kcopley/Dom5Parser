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
    public abstract class PropertyViewModel : ViewModelBase
    {
        public IDEntity Source { get; }
        protected Command _command;
        public string Label { get; }

        public PropertyViewModel(IDEntity e, Command c)
        {
            this.Label = c.ToString();
            this.Source = e;
            this._command = c;
        }

        public PropertyViewModel(string label, IDEntity e, Command c)
        {
            this.Label = label;
            this.Source = e;
            this._command = c;
        }

        public abstract string Value { get; set; }
    }
}
