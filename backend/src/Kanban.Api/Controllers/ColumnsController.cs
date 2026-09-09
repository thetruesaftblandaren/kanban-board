using System.Security.Claims;
using Kanban.Application.Columns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/boards/{boardId}/columns")]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly IColumnService _columnService;

    public ColumnsController(IColumnService columnService)
    {
        _columnService = columnService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid boardId, CreateColumnRequest request)
    {
        var userId = GetUserId();
        var result = await _columnService.CreateColumnAsync(userId, boardId, request);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid boardId)
    {
        var userId = GetUserId();
        var result = await _columnService.GetColumnsForBoardAsync(userId, boardId);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    private Guid GetUserId()
    {
        var sub =  User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}
