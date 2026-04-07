using System.Collections.ObjectModel;
using System.Windows.Input;
using CrmAutomationEngine.Desktop.Models;
using CrmAutomationEngine.Desktop.Services;

namespace CrmAutomationEngine.Desktop.ViewModels;

public class JobsViewModel : ViewModelBase, IAutoLoad
{
    private readonly ApiClient _api;

    private ObservableCollection<ScheduledJob> _items = [];
    private int _page = 1;
    private int _totalPages = 1;
    private bool _isLoading;

    public ObservableCollection<ScheduledJob> Items { get => _items; private set => Set(ref _items, value); }
    public int Page { get => _page; private set { Set(ref _page, value); OnPropertyChanged(nameof(PageDisplay)); } }
    public int TotalPages { get => _totalPages; private set { Set(ref _totalPages, value); OnPropertyChanged(nameof(PageDisplay)); } }
    public string PageDisplay => $"{Page} / {TotalPages}";
    public bool IsLoading { get => _isLoading; private set => Set(ref _isLoading, value); }

    public ICommand LoadCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }

    public JobsViewModel(ApiClient api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async () => await LoadAsync());
        NextPageCommand = new RelayCommand(async () => await LoadAsync(_page + 1), () => _page < _totalPages);
        PrevPageCommand = new RelayCommand(async () => await LoadAsync(_page - 1), () => _page > 1);
    }

    public void BeginLoad() => LoadCommand.Execute(null);

    private async Task LoadAsync(int page = 1)
    {
        IsLoading = true;
        LoadError = null;
        try
        {
            var result = await _api.GetJobsAsync(page);
            if (result is null) return;
            Items = new ObservableCollection<ScheduledJob>(result.Items);
            Page = result.Page;
            TotalPages = (int)Math.Ceiling(result.Total / (double)result.PageSize);
        }
        catch (Exception ex)
        {
            LoadError = $"Failed to load jobs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Maps the int status value from the API to a display string
    public static string StatusLabel(int status) => status switch
    {
        1 => "Scheduled",
        2 => "Processing",
        3 => "Completed",
        4 => "Failed",
        _ => "Unknown"
    };
}
