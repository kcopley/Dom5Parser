using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Dom5Editor
{
    public class WeaponViewModel : IDViewModelBase
    {
        public ObservableCollection<PropertyViewModel> WeaponProperties { get; } = new ObservableCollection<PropertyViewModel>();

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
            get => Weapon._propertyMap.Keys.Except(CoreAttributes).Except(_entity.Properties.Select(p => p.Command));
        }

        public WeaponViewModel(ModViewModel mod, Weapon weapon)
        {
            _entity = weapon;
            Parent = mod;

            CoreAttributes = new List<Command>()
            {
                Command.NAME
            };

            AddPropertyCommand = new RelayCommand(AddProperty);
            RemovePropertyCommand = new RelayCommand<PropertyViewModel>(RemoveProperty);

            _attributeInfos = new Dictionary<Command, AttributeInfo>
            {
                { Command.NAME, new AttributeInfo { PropertyName = nameof(Name), Label = "Name:" } },
            };

            foreach (var kvp in _attributeInfos)
            {
                kvp.Value.ViewModel = CreatePropertyViewModel(kvp.Value.Label, kvp.Key);
            }

            RefreshWeaponProperties();
        }

        private PropertyViewModel CreatePropertyViewModel(string label, Command command)
        {
            if (command == Command.NAME)
                return new StringViewModel(label, _entity, command);
            else
                return new IntPropertyViewModel(label, _entity, command);
        }

        private void AddProperty()
        {
            if (Weapon._propertyMap.TryGetValue(SelectedCommand, out var creator))
            {
                var property = creator.Invoke();
                property.Parent = _entity;
                property.Parse(SelectedCommand, "0", "");
                _entity.AddProperty(property);
                RefreshWeaponProperties();
            }
        }

        private void RemoveProperty(PropertyViewModel propertyVM)
        {
            _entity.RemoveProperty(propertyVM.Command);
            RefreshWeaponProperties();
        }

        private void RefreshWeaponProperties()
        {
            WeaponProperties.Clear();
            foreach (var prop in _entity.Properties)
            {
                if (!CoreAttributes.Contains(prop.Command))
                {
                    WeaponProperties.Add(GetVM(prop));
                }
            }
            OnPropertyChanged(nameof(WeaponProperties));
            //OnPropertyChanged(nameof(AvailableCommands)); //should never change
        }

        public PropertyViewModel GetAttribute(Command command)
        {
            return _attributeInfos.TryGetValue(command, out var info) ? info.ViewModel : null;
        }

        public void SetWeapon(Weapon w)
        {
            this._entity = w;
            RefreshWeaponProperties();
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ID));
            // Notify changes for all core attributes
            foreach (var attributeInfo in _attributeInfos.Values)
            {
                OnPropertyChanged(attributeInfo.PropertyName);
            }
        }

        public Weapon Weapon { get { return _entity as Weapon; } }

        public int ID
        {
            get { return _entity != null ? _entity.ID : -1; }
            set { if (_entity != null) _entity.ID = value; }
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