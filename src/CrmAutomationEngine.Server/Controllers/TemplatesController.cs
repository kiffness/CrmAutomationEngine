using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Controllers;

public class TemplatesController(AppDbContext db, TenantContext tenantContext) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = db.EmailTemplates.OrderBy(t => t.Name);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new PagedResult<EmailTemplate>(items, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var template = await db.EmailTemplates.FindAsync(id);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTemplateRequest request)
    {
        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId,
            Name = request.Name,
            HtmlBody = request.HtmlBody,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = template.Id }, template);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTemplateRequest request)
    {
        var template = await db.EmailTemplates.FindAsync(id);
        if (template is null) return NotFound();
        template.Name = request.Name;
        template.HtmlBody = request.HtmlBody;
        template.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(template);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var template = await db.EmailTemplates.FindAsync(id);
        if (template is null) return NotFound();
        db.EmailTemplates.Remove(template);
        await db.SaveChangesAsync();
        return NoContent();
    }

}

public record CreateTemplateRequest(string Name, string HtmlBody);
public record UpdateTemplateRequest(string Name, string HtmlBody);
