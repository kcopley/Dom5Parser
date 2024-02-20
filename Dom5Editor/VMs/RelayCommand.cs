using System;
using System.Windows.Input;

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

