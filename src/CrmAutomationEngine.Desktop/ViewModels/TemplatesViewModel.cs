using System.Collections.ObjectModel;
using System.Windows.Input;
using CrmAutomationEngine.Desktop.Models;
using CrmAutomationEngine.Desktop.Services;

namespace CrmAutomationEngine.Desktop.ViewModels;

public class TemplatesViewModel : ViewModelBase, IAutoLoad
{
    private readonly ApiClient _api;

    private ObservableCollection<EmailTemplate> _items = [];
    private int _page = 1;
    private int _totalPages = 1;
    private bool _isLoading;

    // Edit form state
    private bool _isEditing;
    private Guid? _editingId;  // null = new template
    private string _editName = string.Empty;
    private string _editHtmlBody = string.Empty;
    private string? _errorMessage;

    public ObservableCollection<EmailTemplate> Items { get => _items; private set => Set(ref _items, value); }
    public int Page { get => _page; private set { Set(ref _page, value); OnPropertyChanged(nameof(PageDisplay)); } }
    public int TotalPages { get => _totalPages; private set { Set(ref _totalPages, value); OnPropertyChanged(nameof(PageDisplay)); } }
    public string PageDisplay => $"{Page} / {TotalPages}";
    public bool IsLoading { get => _isLoading; private set => Set(ref _isLoading, value); }
    public bool IsEditing { get => _isEditing; private set => Set(ref _isEditing, value); }
    public string EditName { get => _editName; set => Set(ref _editName, value); }
    public string EditHtmlBody { get => _editHtmlBody; set => Set(ref _editHtmlBody, value); }
    public string? ErrorMessage { get => _errorMessage; private set => Set(ref _errorMessage, value); }
    public string FormTitle => _editingId is null ? "New Template" : "Edit Template";

    public ICommand LoadCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CancelCommand { get; }

    public TemplatesViewModel(ApiClient api)
    {
        _api = api;
        LoadCommand = new RelayCommand(async () => await LoadAsync());
        NextPageCommand = new RelayCommand(async () => await LoadAsync(_page + 1), () => _page < _totalPages);
        PrevPageCommand = new RelayCommand(async () => await LoadAsync(_page - 1), () => _page > 1);

        AddCommand = new RelayCommand(OpenAddForm);
        EditCommand = new RelayCommand<EmailTemplate>(OpenEditForm);
        SaveCommand = new RelayCommand(async () => await SaveAsync());
        DeleteCommand = new RelayCommand<EmailTemplate>(async t => await DeleteAsync(t));
        CancelCommand = new RelayCommand(CloseForm);
    }

    public void BeginLoad() => LoadCommand.Execute(null);

    private async Task LoadAsync(int page = 1)
    {
        IsLoading = true;
        LoadError = null;
        try
        {
            var result = await _api.GetTemplatesAsync(page);
            if (result is null) return;
            Items = new ObservableCollection<EmailTemplate>(result.Items);
            Page = result.Page;
            TotalPages = (int)Math.Ceiling(result.Total / (double)result.PageSize);
        }
        catch (Exception ex)
        {
            LoadError = $"Failed to load templates: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OpenAddForm()
    {
        _editingId = null;
        EditName = string.Empty;
        EditHtmlBody = string.Empty;
        ErrorMessage = null;
        IsEditing = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private void OpenEditForm(EmailTemplate template)
    {
        _editingId = template.Id;
        EditName = template.Name;
        EditHtmlBody = template.HtmlBody;
        ErrorMessage = null;
        IsEditing = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName)) { ErrorMessage = "Name is required."; return; }

        var body = new { Name = EditName, HtmlBody = EditHtmlBody };
        var response = _editingId is null
            ? await _api.CreateTemplateAsync(body)
            : await _api.UpdateTemplateAsync(_editingId.Value, body);

        if (!response.IsSuccessStatusCode) { ErrorMessage = $"Error: {response.StatusCode}"; return; }

        CloseForm();
        await LoadAsync(_page);
    }

    private async Task DeleteAsync(EmailTemplate template)
    {
        var response = await _api.DeleteTemplateAsync(template.Id);
        if (response.IsSuccessStatusCode)
            await LoadAsync(_page);
    }

    private void CloseForm()
    {
        IsEditing = false;
        ErrorMessage = null;
    }
}
