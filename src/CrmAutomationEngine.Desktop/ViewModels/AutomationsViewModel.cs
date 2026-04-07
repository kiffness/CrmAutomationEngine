using System.Collections.ObjectModel;
using System.Windows.Input;
using CrmAutomationEngine.Desktop.Models;
using CrmAutomationEngine.Desktop.Services;

namespace CrmAutomationEngine.Desktop.ViewModels;

public class AutomationsViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    private ObservableCollection<Automation> _items = [];
    private ObservableCollection<EmailTemplate> _availableTemplates = [];
    private int _page = 1;
    private int _totalPages = 1;
    private bool _isLoading;

    // Edit form state
    private bool _isEditing;
    private Guid? _editingId;
    private string _editName = string.Empty;
    private int _editTrigger = 1;
    private EmailTemplate? _editTemplate;
    private int _editDelayMinutes;
    private string? _errorMessage;

    public ObservableCollection<Automation> Items { get => _items; private set => Set(ref _items, value); }
    public ObservableCollection<EmailTemplate> AvailableTemplates { get => _availableTemplates; private set => Set(ref _availableTemplates, value); }
    public int Page { get => _page; private set { Set(ref _page, value); OnPropertyChanged(nameof(PageDisplay)); } }
    public int TotalPages { get => _totalPages; private set { Set(ref _totalPages, value); OnPropertyChanged(nameof(PageDisplay)); } }
    public string PageDisplay => $"{Page} / {TotalPages}";
    public bool IsLoading { get => _isLoading; private set => Set(ref _isLoading, value); }
    public bool IsEditing { get => _isEditing; private set => Set(ref _isEditing, value); }
    public string EditName { get => _editName; set => Set(ref _editName, value); }
    public int EditTrigger { get => _editTrigger; set => Set(ref _editTrigger, value); }
    public EmailTemplate? EditTemplate { get => _editTemplate; set => Set(ref _editTemplate, value); }
    public int EditDelayMinutes { get => _editDelayMinutes; set => Set(ref _editDelayMinutes, value); }
    public string? ErrorMessage { get => _errorMessage; private set => Set(ref _errorMessage, value); }
    public string FormTitle => _editingId is null ? "New Automation" : "Edit Automation";

    // Trigger options for ComboBox
    public static IReadOnlyList<TriggerOption> TriggerOptions { get; } =
    [
        new(1, "Contact Created"),
        new(2, "Deal Stage Changed")
    ];

    public ICommand LoadCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CancelCommand { get; }

    public AutomationsViewModel(ApiClient api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async () => await LoadAsync());
        NextPageCommand = new RelayCommand(async () => await LoadAsync(_page + 1), () => _page < _totalPages);
        PrevPageCommand = new RelayCommand(async () => await LoadAsync(_page - 1), () => _page > 1);
        AddCommand = new RelayCommand(async () => await OpenAddFormAsync());
        EditCommand = new RelayCommand<Automation>(async a => await OpenEditFormAsync(a));
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        DeleteCommand = new RelayCommand<Automation>(async a => await DeleteAsync(a));
        CancelCommand = new RelayCommand(CloseForm);
    }

    private async Task LoadAsync(int page = 1)
    {
        IsLoading = true;
        try
        {
            var result = await _api.GetAutomationsAsync(page);
            if (result is null) return;
            Items = new ObservableCollection<Automation>(result.Items);
            Page = result.Page;
            TotalPages = (int)Math.Ceiling(result.Total / (double)result.PageSize);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTemplatesIfEmptyAsync()
    {
        if (_availableTemplates.Count > 0) return;
        var result = await _api.GetTemplatesAsync(pageSize: 100);
        if (result is not null)
            AvailableTemplates = new ObservableCollection<EmailTemplate>(result.Items);
    }

    private async Task OpenAddFormAsync()
    {
        await LoadTemplatesIfEmptyAsync();
        _editingId = null;
        EditName = string.Empty;
        EditTrigger = 1;
        EditTemplate = _availableTemplates.FirstOrDefault();
        EditDelayMinutes = 0;
        ErrorMessage = null;
        IsEditing = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private async Task OpenEditFormAsync(Automation automation)
    {
        await LoadTemplatesIfEmptyAsync();
        _editingId = automation.Id;
        EditName = automation.Name;
        EditTrigger = automation.Trigger;
        EditTemplate = _availableTemplates.FirstOrDefault(t => t.Id == automation.EmailTemplateId);
        EditDelayMinutes = automation.DelayMinutes;
        ErrorMessage = null;
        IsEditing = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName)) { ErrorMessage = "Name is required."; return; }
        if (EditTemplate is null) { ErrorMessage = "Template is required."; return; }

        var body = new { Name = EditName, Trigger = EditTrigger, EmailTemplateId = EditTemplate.Id, DelayMinutes = EditDelayMinutes };
        var response = _editingId is null
            ? await _api.CreateAutomationAsync(body)
            : await _api.UpdateAutomationAsync(_editingId.Value, body);

        if (!response.IsSuccessStatusCode) { ErrorMessage = $"Error: {response.StatusCode}"; return; }

        CloseForm();
        await LoadAsync(_page);
    }

    private async Task DeleteAsync(Automation automation)
    {
        var response = await _api.DeleteAutomationAsync(automation.Id);
        if (response.IsSuccessStatusCode)
            await LoadAsync(_page);
    }

    private void CloseForm()
    {
        IsEditing = false;
        ErrorMessage = null;
    }
}

public record TriggerOption(int Value, string Label);
