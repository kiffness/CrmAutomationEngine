using CrmAutomationEngine.Core.Interfaces;
using CrmAutomationEngine.Infrastructure.Email;
using CrmAutomationEngine.Infrastructure.HubSpot;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrmAutomationEngine.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default")));

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddHttpClient<IHubSpotClient, HubSpotClient>();
        services.AddScoped<IEmailService, SendGridEmailService>();

        return services;
    }
}
