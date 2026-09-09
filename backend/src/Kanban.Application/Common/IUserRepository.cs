using Kanban.Domain.Entities;

namespace Kanban.Application.Common;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
