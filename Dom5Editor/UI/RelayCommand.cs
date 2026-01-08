using System;
using System.Windows.Input;

namespace Dom5Editor.UI
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeWithParameter;
        private readonly Action _executeWithoutParameter;
        private readonly Func<object, bool> _canExecute;

        // Constructor for actions with a parameter
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _executeWithParameter = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Constructor for actions without a parameter
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _executeWithoutParameter = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute != null ? new Func<object, bool>(x => canExecute()) : null;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_executeWithParameter != null)
                _executeWithParameter(parameter);
            else
                _executeWithoutParameter?.Invoke();
        }
    }

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
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            // Use pattern matching for safer type checking
            if (parameter is T typedParam)
                return _canExecute(typedParam);

            // Handle null for reference types
            if (parameter == null && !typeof(T).IsValueType)
                return _canExecute(default);

            return true; // No canExecute constraint if type doesn't match
        }

        public void Execute(object parameter)
        {
            // Use pattern matching instead of direct cast for safer execution
            if (parameter is T typedParam)
            {
                _execute(typedParam);
            }
            else if (parameter != null && typeof(T) == typeof(int) && int.TryParse(parameter.ToString(), out int intVal))
            {
                // Special handling for int parameters that might come as strings
                _execute((T)(object)intVal);
            }
            else if (parameter == null && !typeof(T).IsValueType)
            {
                // Handle null for reference types
                _execute(default);
            }
        }
    }
}
