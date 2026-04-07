using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrmAutomationEngine.Desktop.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private string? _loadError;
    public string? LoadError
    {
        get => _loadError;
        protected set { Set(ref _loadError, value); OnPropertyChanged(nameof(HasLoadError)); }
    }
    public bool HasLoadError => !string.IsNullOrEmpty(_loadError);
}

/// <summary>Auto-load contract — MainViewModel calls BeginLoad when navigating to a view.</summary>
public interface IAutoLoad
{
    void BeginLoad();
}
