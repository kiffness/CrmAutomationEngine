using System.Windows.Input;

namespace CrmAutomationEngine.Desktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private object _currentView;
    public object CurrentView
    {
        get => _currentView;
        set
        {
            Set(ref _currentView, value);
            if (value is IAutoLoad al) al.BeginLoad();
        }
    }

    public ICommand ShowContactsCommand { get; }
    public ICommand ShowTemplatesCommand { get; }
    public ICommand ShowAutomationsCommand { get; }
    public ICommand ShowJobsCommand { get; }

    public MainViewModel(ContactsViewModel contacts, TemplatesViewModel templates,
        AutomationsViewModel automations, JobsViewModel jobs)
    {
        ShowContactsCommand = new RelayCommand(() => CurrentView = contacts);
        ShowTemplatesCommand = new RelayCommand(() => CurrentView = templates);
        ShowAutomationsCommand = new RelayCommand(() => CurrentView = automations);
        ShowJobsCommand = new RelayCommand(() => CurrentView = jobs);
        _currentView = contacts;
        contacts.BeginLoad();
    }
}
