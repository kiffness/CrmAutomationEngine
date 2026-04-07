using System.Text;
using CrmAutomationEngine.Infrastructure;
using CrmAutomationEngine.Server.Jobs;
using CrmAutomationEngine.Server.Middleware;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CrmAutomationEngine.Server.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHangfire(config => 
    config.UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Default"))));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<SendEmailJob>();
builder.Services.AddScoped<ContactSyncJob>();
builder.Services.AddScoped<TemplateRenderer>();

var app = builder.Build();

// Apply pending migrations and seed the first tenant on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
await TenantSeeder.SeedAsync(app.Services);

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.MapControllers();
app.MapHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<ContactSyncJob>(
    "contact-sync",
    j => j.RunAsync(CancellationToken.None),
    Cron.Daily);

app.Run();