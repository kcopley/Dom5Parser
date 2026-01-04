using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dom5Edit.Commands;

namespace Dom5Editor.UI.Controls
{
    /// <summary>
    /// Represents an int property item in the list with its state.
    /// </summary>
    public class IntPropertyListItem : INotifyPropertyChanged
    {
        private int _value;

        public Command Command { get; set; }
        public string DisplayName { get; set; }
        public bool IsInherited { get; set; }
        public bool IsModified { get; set; }
        public bool IsSessionEdit { get; set; }

        public int Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged();
                    ValueChanged?.Invoke(this, value);
                }
            }
        }

        public bool CanRemove => !IsInherited;
        public bool CanEdit => true; // Can always edit, even inherited values (creates override)
        public string RemoveTooltip => IsInherited
            ? "Cannot remove inherited property"
            : "Remove this property";

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<int> ValueChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents an available property that can be added.
    /// </summary>
    public class AvailableIntPropertyItem
    {
        public Command Command { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Editor control for managing a list of int properties.
    /// Supports add/remove with protection for inherited values.
    /// </summary>
    public partial class IntPropertyListEditor : UserControl, INotifyPropertyChanged
    {
        private string _newPropertyValue = "0";

        public IntPropertyListEditor()
        {
            InitializeComponent();

            // Set up commands
            RemovePropertyAction = new RelayCommand<IntPropertyListItem>(OnRemoveProperty, CanRemoveProperty);
            AddPropertyAction = new RelayCommand(OnAddProperty, () => CanAdd);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ========================================
        // Dependency Properties
        // ========================================

        /// <summary>
        /// The collection of currently active properties.
        /// </summary>
        public static readonly DependencyProperty ActivePropertiesProperty =
            DependencyProperty.Register(
                nameof(ActiveProperties),
                typeof(ObservableCollection<IntPropertyListItem>),
                typeof(IntPropertyListEditor),
                new PropertyMetadata(null));

        public ObservableCollection<IntPropertyListItem> ActiveProperties
        {
            get => (ObservableCollection<IntPropertyListItem>)GetValue(ActivePropertiesProperty);
            set => SetValue(ActivePropertiesProperty, value);
        }

        /// <summary>
        /// The collection of available properties that can be added.
        /// </summary>
        public static readonly DependencyProperty AvailablePropertiesProperty =
            DependencyProperty.Register(
                nameof(AvailableProperties),
                typeof(ObservableCollection<AvailableIntPropertyItem>),
                typeof(IntPropertyListEditor),
                new PropertyMetadata(null));

        public ObservableCollection<AvailableIntPropertyItem> AvailableProperties
        {
            get => (ObservableCollection<AvailableIntPropertyItem>)GetValue(AvailablePropertiesProperty);
            set => SetValue(AvailablePropertiesProperty, value);
        }

        /// <summary>
        /// The currently selected property to add.
        /// </summary>
        public static readonly DependencyProperty SelectedPropertyToAddProperty =
            DependencyProperty.Register(
                nameof(SelectedPropertyToAdd),
                typeof(AvailableIntPropertyItem),
                typeof(IntPropertyListEditor),
                new PropertyMetadata(null, OnSelectedPropertyChanged));

        public AvailableIntPropertyItem SelectedPropertyToAdd
        {
            get => (AvailableIntPropertyItem)GetValue(SelectedPropertyToAddProperty);
            set => SetValue(SelectedPropertyToAddProperty, value);
        }

        private static void OnSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (IntPropertyListEditor)d;
            // Auto-add when a property is selected from dropdown (default value 1, only when dropdown closes)
            if (e.NewValue is AvailableIntPropertyItem item)
            {
                // Delay the add to avoid issues with scrolling - only trigger when dropdown is closed
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (control.SelectedPropertyToAdd == item)
                    {
                        control.PropertyAdded?.Invoke(control, (item.Command, 1));
                        control.SelectedPropertyToAdd = null;
                    }
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        /// <summary>
        /// The value for the new property to add.
        /// </summary>
        public string NewPropertyValue
        {
            get => _newPropertyValue;
            set
            {
                if (_newPropertyValue != value)
                {
                    _newPropertyValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanAdd));
                }
            }
        }

        public bool CanAdd => SelectedPropertyToAdd != null && int.TryParse(NewPropertyValue, out _);

        // ========================================
        // Commands
        // ========================================

        public ICommand RemovePropertyAction { get; }
        public ICommand AddPropertyAction { get; }

        // ========================================
        // Events
        // ========================================

        /// <summary>
        /// Raised when a property should be added.
        /// </summary>
        public event EventHandler<(Command Command, int Value)> PropertyAdded;

        /// <summary>
        /// Raised when a property should be removed.
        /// </summary>
        public event EventHandler<Command> PropertyRemoved;

        /// <summary>
        /// Raised when a property value is changed.
        /// </summary>
        public event EventHandler<(Command Command, int Value)> PropertyValueChanged;

        // ========================================
        // Command Handlers
        // ========================================

        private void OnAddProperty()
        {
            if (SelectedPropertyToAdd != null && int.TryParse(NewPropertyValue, out int value))
            {
                PropertyAdded?.Invoke(this, (SelectedPropertyToAdd.Command, value));
                SelectedPropertyToAdd = null;
                NewPropertyValue = "0";
            }
        }

        private bool CanRemoveProperty(IntPropertyListItem item)
        {
            return item?.CanRemove ?? false;
        }

        private void OnRemoveProperty(IntPropertyListItem item)
        {
            if (item != null && item.CanRemove)
            {
                PropertyRemoved?.Invoke(this, item.Command);
            }
        }

        private void OnComboBoxPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Prevent mouse wheel from changing selection when dropdown is closed
            var comboBox = sender as ComboBox;
            if (comboBox != null && !comboBox.IsDropDownOpen)
            {
                e.Handled = true;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
