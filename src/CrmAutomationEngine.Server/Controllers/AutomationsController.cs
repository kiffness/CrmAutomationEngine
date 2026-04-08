using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Core.Enums;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Controllers;

public class AutomationsController(AppDbContext db, TenantContext tenantContext) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = db.Automations.Include(a => a.EmailTemplate).OrderBy(a => a.Name);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new PagedResult<Automation>(items, total, page, pageSize));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var automation = await db.Automations.Include(a => a.EmailTemplate).FirstOrDefaultAsync(a => a.Id == id);
        return automation is null ? NotFound() : Ok(automation);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAutomationRequest request)
    {
        var automation = new Automation()
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId,
            Name = request.Name,
            EmailTemplateId = request.EmailTemplateId,
            DelayMinutes = request.DelayMinutes,
        };
        
        db.Automations.Add(automation);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = automation.Id }, automation);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateAutomationRequest request)
    {
        var automation = await db.Automations.FindAsync(id);
        if (automation is null) return NotFound();
        automation.Name = request.Name;
        automation.Trigger = request.Trigger;
        automation.EmailTemplateId = request.EmailTemplateId;
        automation.DelayMinutes = request.DelayMinutes;
        await db.SaveChangesAsync();
        return Ok(automation);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var automation = await db.Automations.FindAsync(id);
        if (automation is null) return NotFound();
        db.Automations.Remove(automation);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record CreateAutomationRequest(string Name, TriggerType Trigger, Guid EmailTemplateId, int DelayMinutes);
public record UpdateAutomationRequest(string Name, TriggerType Trigger, Guid EmailTemplateId, int DelayMinutes);