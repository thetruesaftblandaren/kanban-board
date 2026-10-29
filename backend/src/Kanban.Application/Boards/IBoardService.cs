using Kanban.Application.Common;

namespace Kanban.Application.Boards;

public interface IBoardService
{
    Task<Result<BoardResponse>> CreateBoardAsync(Guid userId, CreateBoardRequest request);
    Task<ICollection<BoardResponse>> GetBoardsForUserAsync(Guid userId);
    Task<Result<BoardResponse>> GetBoardByIdAsync(Guid userId, Guid boardId);
    Task<Result<ICollection<BoardMemberResponse>>> GetMembersAsync(Guid userId, Guid boardId);
    Task<Result<BoardMemberResponse>> ChangeMemberRoleAsync(Guid userId, Guid boardId, Guid memberUserId, ChangeMemberRoleRequest request);
    Task<Result<bool>> RemoveMemberAsync(Guid userId, Guid boardId, Guid memberUserId);
    Task<Result<BoardMemberResponse>> InviteMemberAsync(Guid userId, Guid boardId, InviteMemberRequest request);
    Task<Result<BoardResponse>> RenameBoardAsync(Guid userId, Guid boardId, UpdateBoardRequest request);
    Task<Result<bool>> DeleteBoardAsync(Guid userId, Guid boardId);
}
