using CrmAutomationEngine.Core.Entities;
using CrmAutomationEngine.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmAutomationEngine.Server.Controllers;

public class ContactsController(AppDbContext db) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = db.Contacts.OrderBy(c => c.LastName);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return Ok(new PagedResult<Contact>(items, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var contact = await db.Contacts.FindAsync(id);
        return contact is null ? NotFound() : Ok(contact);
    }
}
