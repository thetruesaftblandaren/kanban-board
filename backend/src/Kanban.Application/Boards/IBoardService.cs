using Kanban.Application.Common;

namespace Kanban.Application.Boards;

public interface IBoardService
{
    Task<Result<BoardResponse>> CreateBoardAsync(Guid userId, CreateBoardRequest request);
    Task<ICollection<BoardResponse>> GetBoardsForUserAsync(Guid userId);
    Task<Result<BoardResponse>> GetBoardByIdAsync(Guid userId, Guid boardId);
}
