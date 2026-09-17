using Kanban.Domain.Entities;

namespace Kanban.Application.Common;

public interface IBoardRepository
{
    Task AddAsync(Board board);
    Task<Board?> GetByIdAsync(Guid id);
    Task<Board?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Board>> GetForUserAsync(Guid userId);
    Task DeleteAsync(Board board);
    Task SaveChangesAsync();
}
