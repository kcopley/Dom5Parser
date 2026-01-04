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
    /// Represents a command item in the list with its state.
    /// </summary>
    public class CommandListItem : INotifyPropertyChanged
    {
        public Command Command { get; set; }
        public string DisplayName { get; set; }
        public bool IsInherited { get; set; }
        public bool IsModified { get; set; }
        public bool IsSessionEdit { get; set; }

        public bool CanRemove => !IsInherited;
        public string RemoveTooltip => IsInherited
            ? "Cannot remove inherited command"
            : "Remove this command";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents an available command that can be added.
    /// </summary>
    public class AvailableCommandItem
    {
        public Command Command { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Editor control for managing a list of command properties.
    /// Supports add/remove with protection for inherited values.
    /// </summary>
    public partial class CommandListEditor : UserControl, INotifyPropertyChanged
    {
        public CommandListEditor()
        {
            InitializeComponent();

            // Set up commands
            RemoveCommandAction = new RelayCommand<CommandListItem>(OnRemoveCommand, CanRemoveCommand);
            AddCommandAction = new RelayCommand(OnAddCommand, () => CanAdd);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ========================================
        // Dependency Properties
        // ========================================

        /// <summary>
        /// The collection of currently active commands.
        /// </summary>
        public static readonly DependencyProperty ActiveCommandsProperty =
            DependencyProperty.Register(
                nameof(ActiveCommands),
                typeof(ObservableCollection<CommandListItem>),
                typeof(CommandListEditor),
                new PropertyMetadata(null));

        public ObservableCollection<CommandListItem> ActiveCommands
        {
            get => (ObservableCollection<CommandListItem>)GetValue(ActiveCommandsProperty);
            set => SetValue(ActiveCommandsProperty, value);
        }

        /// <summary>
        /// The collection of available commands that can be added.
        /// </summary>
        public static readonly DependencyProperty AvailableCommandsProperty =
            DependencyProperty.Register(
                nameof(AvailableCommands),
                typeof(ObservableCollection<AvailableCommandItem>),
                typeof(CommandListEditor),
                new PropertyMetadata(null));

        public ObservableCollection<AvailableCommandItem> AvailableCommands
        {
            get => (ObservableCollection<AvailableCommandItem>)GetValue(AvailableCommandsProperty);
            set => SetValue(AvailableCommandsProperty, value);
        }

        /// <summary>
        /// The currently selected command to add.
        /// </summary>
        public static readonly DependencyProperty SelectedCommandToAddProperty =
            DependencyProperty.Register(
                nameof(SelectedCommandToAdd),
                typeof(AvailableCommandItem),
                typeof(CommandListEditor),
                new PropertyMetadata(null, OnSelectedCommandChanged));

        public AvailableCommandItem SelectedCommandToAdd
        {
            get => (AvailableCommandItem)GetValue(SelectedCommandToAddProperty);
            set => SetValue(SelectedCommandToAddProperty, value);
        }

        private static void OnSelectedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CommandListEditor)d;
            // Auto-add when a command is selected from dropdown (only when dropdown closes)
            if (e.NewValue is AvailableCommandItem item)
            {
                // Delay the add to avoid issues with scrolling - only trigger when dropdown is closed
                control.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (control.SelectedCommandToAdd == item)
                    {
                        control.CommandAdded?.Invoke(control, item.Command);
                        control.SelectedCommandToAdd = null;
                    }
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        public bool CanAdd => SelectedCommandToAdd != null;

        // ========================================
        // Commands
        // ========================================

        public ICommand RemoveCommandAction { get; }
        public ICommand AddCommandAction { get; }

        // ========================================
        // Events
        // ========================================

        /// <summary>
        /// Raised when a command should be added.
        /// </summary>
        public event EventHandler<Command> CommandAdded;

        /// <summary>
        /// Raised when a command should be removed.
        /// </summary>
        public event EventHandler<Command> CommandRemoved;

        // ========================================
        // Command Handlers
        // ========================================

        private void OnAddCommand()
        {
            if (SelectedCommandToAdd != null)
            {
                CommandAdded?.Invoke(this, SelectedCommandToAdd.Command);
                SelectedCommandToAdd = null;
            }
        }

        private bool CanRemoveCommand(CommandListItem item)
        {
            return item?.CanRemove ?? false;
        }

        private void OnRemoveCommand(CommandListItem item)
        {
            if (item != null && item.CanRemove)
            {
                CommandRemoved?.Invoke(this, item.Command);
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

    /// <summary>
    /// Simple relay command implementation.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }

    /// <summary>
    /// Generic relay command implementation.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter is T typedParam)
                return _canExecute?.Invoke(typedParam) ?? true;
            return false;
        }

        public void Execute(object parameter)
        {
            if (parameter is T typedParam)
                _execute(typedParam);
        }
    }
}
