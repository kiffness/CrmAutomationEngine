using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Core.Interfaces;

namespace CrmAutomationEngine.Infrastructure.HubSpot;

public class HubSpotClient : IHubSpotClient
{
    private readonly HttpClient _http;

    public HubSpotClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://api.hubapi.com");
    }

    public async Task<IReadOnlyList<Contact>> GetContactsAsync(string hubSpotToken, CancellationToken ct = default)
    {
        var contacts = new List<Contact>();
        string? after = null;

        do
        {
            var url = "/crm/v3/objects/contacts?properties=firstname,lastname,email,company&limit=100";
            if (after is not null) url += $"&after={after}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", hubSpotToken);

            var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<HubSpotContactsResponse>(cancellationToken: ct);
            if (json is null) break;

            contacts.AddRange(json.Results.Select(r => new Contact
            {
                HubSpotId = r.Id,
                FirstName = r.Properties.GetValueOrDefault("firstname") ?? string.Empty,
                LastName = r.Properties.GetValueOrDefault("lastname") ?? string.Empty,
                Email = r.Properties.GetValueOrDefault("email") ?? string.Empty,
                CompanyName = r.Properties.GetValueOrDefault("company") ?? string.Empty,
                LastSyncedAt = DateTime.UtcNow
            }));
            after = json.Paging?.Next?.After;
        }
        while (after is not null);

        return contacts;
    }

    public async Task<Contact?> GetContactAsync(string hubSpotToken, string hubSpotId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/crm/v3/objects/contacts/{hubSpotId}?properties=firstname,lastname,email,company");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", hubSpotToken);

        var response = await _http.SendAsync(request, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<HubSpotContactResult>(cancellationToken: ct);
        if (json is null) return null;

        return new Contact
        {
            HubSpotId = json.Id,
            FirstName = json.Properties.GetValueOrDefault("firstname") ?? string.Empty,
            LastName = json.Properties.GetValueOrDefault("lastname") ?? string.Empty,
            Email = json.Properties.GetValueOrDefault("email") ?? string.Empty,
            CompanyName = json.Properties.GetValueOrDefault("company") ?? string.Empty,
            LastSyncedAt = DateTime.UtcNow
        };
    }
}

file record HubSpotContactsResponse(
    List<HubSpotContactResult> Results,
    HubSpotPaging? Paging);

file record HubSpotContactResult(
    string Id,
    Dictionary<string, string?> Properties);

file record HubSpotPaging(HubSpotPagingNext? Next);
file record HubSpotPagingNext(string? After);
