using System.Windows.Input;

namespace CrmAutomationEngine.Desktop.ViewModels;

public class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? _) => canExecute?.Invoke() ?? true;
    public void Execute(object? _) => execute();
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) => parameter is T t && (canExecute?.Invoke(t) ?? true);
    public void Execute(object? parameter) { if (parameter is T t) execute(t); }
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
