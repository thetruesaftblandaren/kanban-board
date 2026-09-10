using Kanban.Application.Common;

namespace Kanban.Application.Cards;

public interface ICardService
{
    Task<Result<CardResponse>> CreateCardAsync(Guid userId, Guid boardId, Guid columnId, CreateCardRequest request);
    Task<Result<CardResponse>> MoveCardAsync(Guid userId, Guid boardId, Guid cardId, MoveCardRequest request);
}
