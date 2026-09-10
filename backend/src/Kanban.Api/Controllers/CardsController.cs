using System.Security.Claims;
using Kanban.Application.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kanban.Api.Controllers;

[ApiController]
[Route("api/boards/{boardId}/columns/{columnId}/cards")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid boardId, Guid columnId, CreateCardRequest request)
    {
        var userId = GetUserId();
        var result = await _cardService.CreateCardAsync(userId, boardId, columnId, request);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid boardId, Guid columnId)
    {
        var userId = GetUserId();
        var result = await _cardService.GetCardsForColumnAsync(userId, boardId, columnId);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    [HttpPatch("~/api/boards/{boardId}/cards/{cardId}/move")]
    public async Task<IActionResult> Move(Guid boardId, Guid cardId, MoveCardRequest request)
    {
        var userId = GetUserId();
        var result = await _cardService.MoveCardAsync(userId, boardId, cardId, request);

        return result.Success ? Ok(result.Value) : NotFound(result.Errors);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}
