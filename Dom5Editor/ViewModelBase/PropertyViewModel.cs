using Dom5Edit.Commands;
using Dom5Edit.Entities;

namespace Dom5Editor.VMs
{
    public abstract class PropertyViewModel : ViewModelBase
    {
        public IDEntity Source { get; }
        public Command Command { get; private set; }
        public string Label { get; }

        public PropertyViewModel(IDEntity e, Command c)
        {
            this.Label = c.ToString();
            this.Source = e;
            this.Command = c;
        }

        public PropertyViewModel(string label, IDEntity e, Command c)
        {
            this.Label = label;
            this.Source = e;
            this.Command = c;
        }

        public abstract string Value { get; set; }
    }
}
