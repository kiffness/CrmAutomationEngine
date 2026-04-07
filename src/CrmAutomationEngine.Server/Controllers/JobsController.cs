using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Controllers;

public class JobsController(AppDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = db.ScheduledJobs.OrderByDescending(t => t.CreatedAt);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new PagedResult<ScheduledJob>(items, total, page, pageSize));
    }
    
    
}
