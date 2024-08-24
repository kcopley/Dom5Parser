using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Dom5Editor
{
    public class SiteViewModel : IDViewModelBase
    {
        public ObservableCollection<PropertyViewModel> SiteProperties { get; } = new ObservableCollection<PropertyViewModel>();

        private readonly Dictionary<Command, AttributeInfo> _attributeInfos;

        public ICommand AddPropertyCommand { get; }
        public ICommand RemovePropertyCommand { get; }

        private Command _selectedCommand;
        public Command SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                _selectedCommand = value;
                OnPropertyChanged(nameof(SelectedCommand));
            }
        }

        public IEnumerable<Command> AvailableCommands
        {
            get => Site._propertyMap.Keys.Except(CoreAttributes).Except(_entity.Properties.Select(p => p.Command));
        }

        public SiteViewModel(ModViewModel mod, Site site)
        {
            _entity = site;
            Parent = mod;

            CoreAttributes = new List<Command>()
            {
                Command.NAME
            };

            AddPropertyCommand = new RelayCommand(AddProperty);
            RemovePropertyCommand = new RelayCommand<PropertyViewModel>(RemoveProperty);

            _attributeInfos = new Dictionary<Command, AttributeInfo>
            {
                { Command.NAME, new AttributeInfo { PropertyName = nameof(Name), Label = "Name:" } }
            };

            foreach (var kvp in _attributeInfos)
            {
                kvp.Value.ViewModel = new StringViewModel(kvp.Value.Label, _entity, kvp.Key);
            }

            RefreshSiteProperties();
        }

        private void AddProperty()
        {
            if (Site._propertyMap.TryGetValue(SelectedCommand, out var creator))
            {
                var property = creator.Invoke();
                property.Parent = _entity;
                property.Parse(SelectedCommand, "0", "");
                _entity.AddProperty(property);
                RefreshSiteProperties();
            }
        }

        private void RemoveProperty(PropertyViewModel propertyVM)
        {
            _entity.RemoveProperty(propertyVM.Command);
            RefreshSiteProperties();
        }

        private void RefreshSiteProperties()
        {
            SiteProperties.Clear();
            foreach (var prop in _entity.Properties)
            {
                if (!CoreAttributes.Contains(prop.Command))
                {
                    SiteProperties.Add(GetVM(prop));
                }
            }
            OnPropertyChanged(nameof(SiteProperties));
            OnPropertyChanged(nameof(AvailableCommands));
        }

        public PropertyViewModel GetAttribute(Command command)
        {
            return _attributeInfos.TryGetValue(command, out var info) ? info.ViewModel : null;
        }

        public void SetSite(Site s)
        {
            this._entity = s;
            RefreshSiteProperties();
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ID));
        }

        public Site Site { get { return _entity as Site; } }

        public int ID
        {
            get
            {
                return _entity != null ? _entity.ID : -1;
            }
            set
            {
                if (_entity != null) _entity.ID = value;
            }
        }

        public StringViewModel Name => GetAttribute(Command.NAME) as StringViewModel;

        public override string DisplayName
        {
            get
            {
                if (_entity != null)
                {
                    return "(" + _entity.ID + ") " + _entity.Name;
                }
                else
                {
                    return "<No Name>";
                }
            }
        }
    }
}
