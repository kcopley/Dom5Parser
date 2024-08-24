using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
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

        private CollectionViewSource _sitePropertiesSource;
        public ICollectionView SitePropertiesView => _sitePropertiesSource.View;

        private CollectionViewSource _siteCommandsSource;
        public ICollectionView SiteCommandsView => _siteCommandsSource.View;

        public ICommand AddPropertyCommand { get; }

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

        public SiteViewModel(ModViewModel mod, Site site)
        {
            _entity = site;
            Parent = mod;

            CoreAttributes = new List<Command>()
            {
                Command.NAME
            };

            _siteCommandsSource = new CollectionViewSource { Source = Site._propertyMap.Keys };
            _siteCommandsSource.Filter += (s, e) =>
            {
                e.Accepted = e.Item is Command item && !CoreAttributes.Contains(item);
            };
            _sitePropertiesSource = new CollectionViewSource { Source = AllProperties };
            _sitePropertiesSource.Filter += (s, e) =>
            {
                e.Accepted = e.Item is PropertyViewModel item && !CoreAttributes.Contains(item.Command);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);

        }

        private void AddProperty()
        {
            if (Site._propertyMap.TryGetValue(SelectedCommand, out var creator))
            {
                var property = creator.Invoke();
                property.Parent = _entity;
                property.Parse(SelectedCommand, "0", "");
                var vm = GetVM(property);
                AllProperties.Add(vm);
            }
        }

        public void SetSite(Site s)
        {
            this._entity = s;
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

        public NameViewModel Name
        {
            get
            {
                return new NameViewModel(_entity, Command.NAME);
            }
        }

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
