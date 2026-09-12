using Kanban.Application.Common;

namespace Kanban.Application.Cards;

public class CardService : ICardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly INotificationService _notificationService;

    public CardService(IBoardRepository boardRepository, INotificationService notificationService)
    {
        _boardRepository = boardRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<CardResponse>> CreateCardAsync(Guid userId, Guid boardId, Guid columnId, CreateCardRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
        {
            return Result<CardResponse>.Fail("Board not found.");
        }

        if (!board.IsMember(userId))
        {
            return Result<CardResponse>.Fail("You do not have access to this board.");
        }

        try
        {
            var card = board.AddCard(columnId, request.Title, request.Description);
            await _boardRepository.SaveChangesAsync();

            return Result<CardResponse>.Ok(new CardResponse(card.Id, card.Title, card.Description, card.ColumnId, card.Order, card.CreatedAt));
        }
        catch (InvalidOperationException ex)
        {
            return Result<CardResponse>.Fail(ex.Message);
        }
    }

    public async Task<Result<ICollection<CardResponse>>> GetCardsForColumnAsync(Guid userId, Guid boardId, Guid columnId)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
            return Result<ICollection<CardResponse>>.Fail("Board not found.");

        if (!board.IsMember(userId))
            return Result<ICollection<CardResponse>>.Fail("You do not have access to this board.");

        var column = board.Columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            return Result<ICollection<CardResponse>>.Fail("Column not found on this board.");

        var response = column.Cards
            .OrderBy(c => c.Order)
            .Select(c => new CardResponse(c.Id, c.Title, c.Description, c.ColumnId, c.Order, c.CreatedAt))
            .ToList();

        return Result<ICollection<CardResponse>>.Ok((ICollection<CardResponse>)response);
    }

    public async Task<Result<CardResponse>> MoveCardAsync(Guid userId, Guid boardId, Guid cardId, MoveCardRequest request)
    {
        var board = await _boardRepository.GetByIdWithDetailsAsync(boardId);
        if (board is null)
        {
            return Result<CardResponse>.Fail("Board not found.");
        }

        if (!board.IsMember(userId))
        {
            return Result<CardResponse>.Fail("You do not have access to this board.");
        }

        try
        {
            board.MoveCard(cardId, request.TargetColumnId, request.NewOrder);
            await _boardRepository.SaveChangesAsync();

            var movedCard = board.Columns
                .SelectMany(c => c.Cards)
                .First(c => c.Id == cardId);

            await _notificationService.NotifyCardMovedAsync(boardId, movedCard.Id, movedCard.ColumnId, movedCard.Order);

            return Result<CardResponse>.Ok(new CardResponse(movedCard.Id, movedCard.Title, movedCard.Description, movedCard.ColumnId, movedCard.Order, movedCard.CreatedAt));
        }
        catch (InvalidOperationException ex)
        {
            return Result<CardResponse>.Fail(ex.Message);
        }
    }
}
