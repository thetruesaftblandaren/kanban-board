using System.Security.Claims;
using Kanban.Application.Boards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/boards")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBoardRequest request)
    {
        var userId = GetUserId();
        var result = await _boardService.CreateBoardAsync(userId, request);

        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.Errors);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var boards = await _boardService.GetBoardsForUserAsync(userId);

        return Ok(boards);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var result = await _boardService.GetBoardByIdAsync(userId, id);
        
        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}
