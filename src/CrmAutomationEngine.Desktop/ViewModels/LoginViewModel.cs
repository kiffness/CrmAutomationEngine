using System.Windows.Input;
using CrmAutomationEngine.Desktop.Services;

namespace CrmAutomationEngine.Desktop.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly string _serverUrl;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => Set(ref _email, value);
    }

    private string _error = string.Empty;
    public string Error
    {
        get => _error;
        set { Set(ref _error, value); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(_error);

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => Set(ref _isBusy, value);
    }

    public ICommand LoginCommand { get; }

    public event Action<string>? LoginSucceeded;

    public LoginViewModel(string serverUrl)
    {
        _serverUrl = serverUrl;
        LoginCommand = new AsyncRelayCommand<string>(DoLoginAsync);
    }

    private async Task DoLoginAsync(string? password)
    {
        Error = string.Empty;
        IsBusy = true;
        try
        {
            var token = await ApiClient.LoginAsync(_serverUrl, Email, password ?? string.Empty);
            LoginSucceeded?.Invoke(token);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
