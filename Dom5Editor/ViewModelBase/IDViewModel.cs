using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Dom5Editor
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class IDViewModelBase : ViewModelBase, INotifyPropertyChanged, IDisposable
    {
        protected IDEntity _entity;
        public ModViewModel Parent { get; protected set; }
        public List<Command> CoreAttributes = new List<Command>() {};

        private ObservableCollection<PropertyViewModel> _properties;
        public ObservableCollection<PropertyViewModel> AllProperties {
            get
            {
                if (_properties == null)
                {
                    ObservableCollection<PropertyViewModel> list = new ObservableCollection<PropertyViewModel>();
                    foreach (var prop in _entity.Properties)
                    {
                        list.Add(GetVM(prop));
                    }
                    _properties = list;
                }
                return _properties;
            }
        }

        private void OnRequestRemove(PropertyViewModel viewModel)
        {
            AllProperties.Remove(viewModel);
        }

        protected PropertyViewModel GetVM(Property p)
        {
            PropertyViewModel viewModel = null; // Variable to hold the assigned view model
            var t = p.GetType();

            if (t.InheritsFrom(typeof(IntProperty)) || t.Equals(typeof(IntProperty)))
            {
                viewModel = new IntPropertyViewModel(p.Command.ToString(), _entity, p.Command);
            }
            else if (t.InheritsFrom(typeof(StringProperty)) || t.Equals(typeof(StringProperty)))
            {
                viewModel = new StringViewModel(p.Command.ToString(), _entity, p.Command);
            }
            else if (t.InheritsFrom(typeof(CommandProperty)) || t.Equals(typeof(CommandProperty)))
            {
                viewModel = new CommandViewModel(p.Command.ToString(), _entity, p.Command);
            }
            else if (t.InheritsFrom(typeof(MonsterOrMontagRef)) || t.Equals(typeof(MonsterOrMontagRef)))
            {
                viewModel = new MonsterRefViewModel(Parent, this, _entity, p as MonsterOrMontagRef);
            }
            else if (t.InheritsFrom(typeof(WeaponRef)) || t.Equals(typeof(WeaponRef)))
            {
                viewModel = new WeaponRefViewModel(Parent, this, _entity, p as WeaponRef);
            }
            else if (t.InheritsFrom(typeof(ArmorRef)) || t.Equals(typeof(ArmorRef)))
            {
                viewModel = new ArmorRefViewModel(Parent, this, _entity, p as ArmorRef);
            }
            else if (t.InheritsFrom(typeof(StringOrIDRef)) || t.Equals(typeof(StringOrIDRef)))
            {
                viewModel = new StringOrIDRefViewModel(p.Command.ToString(), _entity, p.Command); // Assuming you have a StringOrIDRefViewModel
            }
            else
            {
                viewModel = new CommandViewModel(p.Command.ToString(), _entity, p.Command); // Default case
            }

            viewModel.RequestRemove += OnRequestRemove;
            return viewModel; // Return the assigned view model
        }

        /*
         * public CopyStatsRefViewModel CopyRef
            {
                get { return new CopyStatsRefViewModel(this, "Copies From:", _entity, Command.COPYSTATS); }
            }
         */
    }
}