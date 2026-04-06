using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Automation> Automations => Set<Automation>();
    public DbSet<AutomationEnrolment> AutomationEnrolments => Set<AutomationEnrolment>();
    public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Contact>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<EmailTemplate>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Automation>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<AutomationEnrolment>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ScheduledJob>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
    }
}
