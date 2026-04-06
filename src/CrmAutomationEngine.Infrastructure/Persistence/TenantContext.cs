using CrmAutomationEngine.Core.Interfaces;

namespace CrmAutomationEngine.Infrastructure.Persistence;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
}
