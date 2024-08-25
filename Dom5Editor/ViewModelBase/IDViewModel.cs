using Dom5Edit.Commands;
using Dom5Edit.Entities;
using Dom5Edit.Props;
using Dom5Editor.VMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace Dom5Editor
{
    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class IDViewModelBase : ViewModelBase
    {
        protected IDEntity _entity;
        public ModViewModel Parent { get; protected set; }
        public List<Command> CoreAttributes { get; protected set; } = new List<Command>();

        internal Dictionary<Command, AttributeInfo> _attributeInfos;

        public ObservableCollection<PropertyViewModel> EntityProperties { get; } = new ObservableCollection<PropertyViewModel>();

        public ICommand AddPropertyCommand { get; protected set; }
        public ICommand RemovePropertyCommand { get; protected set; }

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
            get => GetPropertyMap().Keys.Except(CoreAttributes).Except(_entity.Properties.Select(p => p.Command));
        }

        protected IDViewModelBase(ModViewModel mod, IDEntity entity)
        {
            _entity = entity;
            Parent = mod;

            AddPropertyCommand = new RelayCommand(AddProperty);
            RemovePropertyCommand = new RelayCommand<PropertyViewModel>(RemoveProperty);

            _attributeInfos = new Dictionary<Command, AttributeInfo>();

            InitializeAttributeInfos();
            RefreshEntityProperties();
        }

        protected abstract Dictionary<Command, Func<Property>> GetPropertyMap();

        protected virtual void InitializeAttributeInfos()
        {
            _attributeInfos[Command.NAME] = new AttributeInfo { PropertyName = nameof(Name), Label = "Name:" };
            foreach (var kvp in _attributeInfos)
            {
                kvp.Value.ViewModel = CreatePropertyViewModel(kvp.Value.Label, kvp.Key);
            }
        }

        protected virtual PropertyViewModel CreatePropertyViewModel(string label, Command command)
        {
            if (command == Command.NAME)
                return new StringViewModel(label, _entity, command);
            else
                return new IntPropertyViewModel(label, _entity, command);
        }

        protected virtual void AddProperty()
        {
            if (GetPropertyMap().TryGetValue(SelectedCommand, out var creator))
            {
                var property = creator.Invoke();
                property.Parent = _entity;
                property.Parse(SelectedCommand, "0", "");
                _entity.AddProperty(property);
                RefreshEntityProperties();
            }
        }

        protected virtual void RemoveProperty(PropertyViewModel propertyVM)
        {
            _entity.RemoveProperty(propertyVM.Command);
            RefreshEntityProperties();
        }

        protected virtual void RefreshEntityProperties()
        {
            EntityProperties.Clear();
            foreach (var prop in _entity.Properties)
            {
                if (!CoreAttributes.Contains(prop.Command))
                {
                    EntityProperties.Add(GetVM(prop));
                }
            }
            OnPropertyChanged(nameof(EntityProperties));
            OnPropertyChanged(nameof(AvailableCommands));
        }

        public PropertyViewModel GetAttribute(Command command)
        {
            return _attributeInfos.TryGetValue(command, out var info) ? info.ViewModel : null;
        }

        public void SetEntity(IDEntity entity)
        {
            this._entity = entity;
            RefreshEntityProperties();
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ID));
            foreach (var attributeInfo in _attributeInfos.Values)
            {
                OnPropertyChanged(attributeInfo.PropertyName);
            }
        }

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

        protected virtual PropertyViewModel GetVM(Property p, string label = null)
        {
            label = label ?? p.Command.ToString();
            var propertyType = p.GetType();

            if (propertyType == typeof(IntProperty) || propertyType.IsSubclassOf(typeof(IntProperty)))
            {
                return new IntPropertyViewModel(label, _entity, p.Command);
            }
            else if (propertyType == typeof(IntIntProperty) || propertyType.IsSubclassOf(typeof(IntIntProperty)))
            {
                return new IntIntPropertyViewModel(label, _entity, p.Command);
            }
            else if (propertyType == typeof(StringProperty) || propertyType.IsSubclassOf(typeof(StringProperty)))
            {
                return new StringViewModel(label, _entity, p.Command);
            }
            else if (propertyType == typeof(CommandProperty) || propertyType.IsSubclassOf(typeof(CommandProperty)))
            {
                return new CommandViewModel(label, _entity, p.Command);
            }
            else if (propertyType == typeof(MonsterOrMontagRef) || propertyType.IsSubclassOf(typeof(MonsterOrMontagRef)))
            {
                return new MonsterRefViewModel(Parent, this, _entity, p as MonsterOrMontagRef);
            }
            else if (propertyType == typeof(WeaponRef) || propertyType.IsSubclassOf(typeof(WeaponRef)))
            {
                return new WeaponRefViewModel(Parent, this, _entity, p as WeaponRef);
            }
            else if (propertyType == typeof(ArmorRef) || propertyType.IsSubclassOf(typeof(ArmorRef)))
            {
                return new ArmorRefViewModel(Parent, this, _entity, p as ArmorRef);
            }
            else if (propertyType == typeof(StringOrIDRef) || propertyType.IsSubclassOf(typeof(StringOrIDRef)))
            {
                return new StringOrIDRefViewModel(label, _entity, p.Command);
            }
            else if (propertyType == typeof(NameProperty) || propertyType.IsSubclassOf(typeof(NameProperty)))
            {
                return new NameViewModel(_entity, p.Command);
            }

            // Default case: use CommandViewModel if no specific type is matched
            return new CommandViewModel(label, _entity, p.Command);
        }
    }

    class AttributeInfo
    {
        public string PropertyName { get; set; }
        public string Label { get; set; }
        public PropertyViewModel ViewModel { get; set; }
    }
}