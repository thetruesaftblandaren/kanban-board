using Kanban.Domain.Entities;

namespace Kanban.Application.Common;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task SaveChangesAsync();
}
