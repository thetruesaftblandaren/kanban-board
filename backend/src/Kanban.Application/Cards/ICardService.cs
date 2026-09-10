using Kanban.Application.Common;

namespace Kanban.Application.Cards;

public interface ICardService
{
    Task<Result<CardResponse>> CreateCardAsync(Guid userId, Guid boardId, Guid columnId, CreateCardRequest request);
    Task<Result<ICollection<CardResponse>>> GetCardsForColumnAsync(Guid userId, Guid boardId, Guid columnId);
    Task<Result<CardResponse>> MoveCardAsync(Guid userId, Guid boardId, Guid cardId, MoveCardRequest request);
}
