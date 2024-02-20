using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
            RemoveCommand = new RelayCommand(RemoveItem);
        }

        public PropertyViewModel(string label, IDEntity e, Command c)
        {
            this.Label = label;
            this.Source = e;
            this.Command = c;
            RemoveCommand = new RelayCommand(RemoveItem);
        }

        public ICommand RemoveCommand { get; private set; }

        public event Action<PropertyViewModel> RequestRemove;
        private void RemoveItem(object parameter)
        {
            RequestRemove?.Invoke(this);
        }

        public abstract string Value { get; set; }
    }
}
