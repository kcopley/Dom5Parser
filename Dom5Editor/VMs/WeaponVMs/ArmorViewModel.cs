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
    public class ArmorViewModel : IDViewModelBase
    {
        public ObservableCollection<PropertyViewModel> ArmorProperties { get; } = new ObservableCollection<PropertyViewModel>();

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
            get => Armor._propertyMap.Keys.Except(CoreAttributes).Except(_entity.Properties.Select(p => p.Command));
        }

        public ArmorViewModel(ModViewModel mod, Armor armor)
        {
            _entity = armor;
            Parent = mod;

            CoreAttributes = new List<Command>()
            {
                Command.NAME,
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

            RefreshArmorProperties();
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
            if (Armor._propertyMap.TryGetValue(SelectedCommand, out var creator))
            {
                var property = creator.Invoke();
                property.Parent = _entity;
                property.Parse(SelectedCommand, "0", "");
                _entity.AddProperty(property);
                RefreshArmorProperties();
            }
        }

        private void RemoveProperty(PropertyViewModel propertyVM)
        {
            _entity.RemoveProperty(propertyVM.Command);
            RefreshArmorProperties();
        }

        private void RefreshArmorProperties()
        {
            ArmorProperties.Clear();
            foreach (var prop in _entity.Properties)
            {
                if (!CoreAttributes.Contains(prop.Command))
                {
                    ArmorProperties.Add(GetVM(prop));
                }
            }
            OnPropertyChanged(nameof(ArmorProperties));
            OnPropertyChanged(nameof(AvailableCommands));
        }

        public PropertyViewModel GetAttribute(Command command)
        {
            return _attributeInfos.TryGetValue(command, out var info) ? info.ViewModel : null;
        }

        public void SetArmor(Armor a)
        {
            this._entity = a;
            RefreshArmorProperties();
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ID));
            // Notify changes for all core attributes
            foreach (var attributeInfo in _attributeInfos.Values)
            {
                OnPropertyChanged(attributeInfo.PropertyName);
            }
        }

        public Armor Armor { get { return _entity as Armor; } }

        public int ID
        {
            get { return _entity != null ? _entity.ID : -1; }
            set { if (_entity != null) _entity.ID = value; }
        }

        public NameViewModel Name => GetAttribute(Command.NAME) as NameViewModel;

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