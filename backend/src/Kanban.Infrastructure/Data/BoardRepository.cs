using Kanban.Application.Common;
using Kanban.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Infrastructure.Data;

public class BoardRepository : IBoardRepository
{
    private readonly AppDbContext _context;

    public BoardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Board board)
    {
        await _context.Boards.AddAsync(board);
    }

    public async Task AddMemberAsync(BoardMember member)
    {
        await _context.BoardMembers.AddAsync(member);
    }

    public Task<Board?> GetByIdAsync(Guid id)
    {
        return _context.Boards.FirstOrDefaultAsync(b => b.Id == id);
    }

    public Task<List<Board>> GetForUserAsync(Guid userId)
    {
        return _context.Boards.Where(b => _context.BoardMembers.Any(bm => bm.BoardId == b.Id && bm.UserId == userId)).ToListAsync();
    }

    public Task<bool> IsUserMemberAsync(Guid boardId, Guid userId)
    {
        return _context.BoardMembers.AnyAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
