using Kanban.Domain.Entities;

namespace Kanban.Application.Common;

public interface IBoardRepository
{
    Task AddAsync(Board board);
    Task<Board?> GetByIdAsync(Guid id);
    Task<List<Board>> GetForUserAsync(Guid userId);
    Task AddMemberAsync(BoardMember member);
    Task<bool> IsUserMemberAsync(Guid boardId, Guid userId);
    Task SaveChangesAsync();
}
