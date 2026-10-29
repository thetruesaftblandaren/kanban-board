using Kanban.Application.Common;
using Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(User user) => await _context.DomainUsers.AddAsync(user);
    public Task<User?> GetByIdAsync(Guid id) => _context.DomainUsers.FindAsync(id).AsTask();

    public Task<List<User>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        return _context.DomainUsers.Where(u => idList.Contains(u.Id)).ToListAsync();
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
