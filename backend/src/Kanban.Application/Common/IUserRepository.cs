namespace Kanban.Application.Common;

using Kanban.Domain.Entities;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
