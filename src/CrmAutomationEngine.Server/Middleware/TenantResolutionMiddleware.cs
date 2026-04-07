using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, AppDbContext db, TenantContext tenantContext)
    {
        // Webhook and auth routes handle their own resolution — skip middleware
        if (context.Request.Path.StartsWithSegments("/api/webhook") ||
            context.Request.Path.StartsWithSegments("/api/auth"))
        {
            await _next(context);
            return;
        }

        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
        if (tenantIdClaim is null || !Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            context.Response.StatusCode = 401;
            return;
        }

        var tenant = await db.Tenants.FindAsync(tenantId);
        if (tenant is null) { context.Response.StatusCode = 401; return; }

        tenantContext.TenantId = tenant.Id;
        await _next(context);
    }
}