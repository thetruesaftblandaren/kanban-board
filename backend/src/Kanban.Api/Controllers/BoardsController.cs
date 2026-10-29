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

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var result = await _boardService.GetMembersAsync(GetUserId(), id);
        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> InviteMember(Guid id, InviteMemberRequest request)
    {
        var result = await _boardService.InviteMemberAsync(GetUserId(), id, request);
        return result.Success ? Ok(result.Success) : NotFound(result.Errors);
    }

    [HttpPut("{id}/members/{memberUserId}")]
    public async Task<ActionResult> ChangeMemberRole(Guid id, Guid memberUserId, ChangeMemberRoleRequest request)
    {
        var result = await _boardService.ChangeMemberRoleAsync(GetUserId(), id, memberUserId, request);
        return result.Success ? Ok(result.Success) : NotFound(result.Errors);
    }

    [HttpDelete("{id}/members/{memberUserId}")]
    public async Task<ActionResult> RemoveMember(Guid id, Guid memberUserId)
    {
        var result = await _boardService.RemoveMemberAsync(GetUserId(), id, memberUserId);
        return result.Success ? NoContent() : BadRequest(result.Errors);
    }

    [HttpPut("{Id}")]
    public async Task<IActionResult> Update(Guid id, UpdateBoardRequest request)
    {
        var userId = GetUserId();
        var result = await _boardService.RenameBoardAsync(userId, id, request);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var result = await _boardService.DeleteBoardAsync(userId, id);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}
