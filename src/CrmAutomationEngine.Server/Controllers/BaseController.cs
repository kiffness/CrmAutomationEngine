using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmAutomationEngine.Server.Controllers;

[ApiController]
[Route("api/[Controller]")]
[Authorize]
public class BaseController : ControllerBase
{
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);