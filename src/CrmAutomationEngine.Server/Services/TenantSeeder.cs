using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Services;

public static class TenantSeeder
{
    /// <summary>
    /// Creates the first tenant if the Tenants table is empty and Seed config is present.
    /// Safe to call on every startup — no-ops if a tenant already exists.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (await db.Tenants.AnyAsync()) return;

        var name     = config["Seed:TenantName"];
        var email    = config["Seed:AdminEmail"];
        var password = config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
            return;  // Seed config not present — skip silently

        var hasher = new PasswordHasher<string>();

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            AdminEmail = email,
            PasswordHash = hasher.HashPassword(email, password),
            ApiKey = Guid.NewGuid().ToString("N"),
            HubSpotToken = config["Seed:HubSpotToken"] ?? string.Empty,
            HubSpotClientSecret = config["Seed:HubSpotClientSecret"] ?? string.Empty
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();
    }
}
