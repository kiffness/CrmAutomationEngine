using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CrmAutomationEngine.Desktop.Models;
using Microsoft.Extensions.Configuration;

namespace CrmAutomationEngine.Desktop.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(IConfiguration config)
    {
        var serverUrl = config["ServerUrl"] ?? throw new InvalidOperationException("ServerUrl not configured");
        var token = config["Jwt:Token"] ?? string.Empty;
        _http = new HttpClient { BaseAddress = new Uri(serverUrl) };
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    // Contacts
    public Task<PagedResult<Contact>?> GetContactsAsync(int page = 1, int pageSize = 50) =>
        _http.GetFromJsonAsync<PagedResult<Contact>>($"api/contacts?page={page}&pageSize={pageSize}");

    // Templates
    public Task<PagedResult<EmailTemplate>?> GetTemplatesAsync(int page = 1, int pageSize = 50) =>
        _http.GetFromJsonAsync<PagedResult<EmailTemplate>>($"api/templates?page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> CreateTemplateAsync(object body) =>
        _http.PostAsJsonAsync("api/templates", body);

    public Task<HttpResponseMessage> UpdateTemplateAsync(Guid id, object body) =>
        _http.PutAsJsonAsync($"api/templates/{id}", body);

    public Task<HttpResponseMessage> DeleteTemplateAsync(Guid id) =>
        _http.DeleteAsync($"api/templates/{id}");

    // Automations
    public Task<PagedResult<Automation>?> GetAutomationsAsync(int page = 1, int pageSize = 50) =>
        _http.GetFromJsonAsync<PagedResult<Automation>>($"api/automations?page={page}&pageSize={pageSize}");

    public Task<HttpResponseMessage> CreateAutomationAsync(object body) =>
        _http.PostAsJsonAsync("api/automations", body);

    public Task<HttpResponseMessage> UpdateAutomationAsync(Guid id, object body) =>
        _http.PutAsJsonAsync($"api/automations/{id}", body);

    public Task<HttpResponseMessage> DeleteAutomationAsync(Guid id) =>
        _http.DeleteAsync($"api/automations/{id}");

    // Jobs
    public Task<PagedResult<ScheduledJob>?> GetJobsAsync(int page = 1, int pageSize = 50) =>
        _http.GetFromJsonAsync<PagedResult<ScheduledJob>>($"api/jobs?page={page}&pageSize={pageSize}");
}