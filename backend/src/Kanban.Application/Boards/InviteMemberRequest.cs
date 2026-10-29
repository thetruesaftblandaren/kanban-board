using Kanban.Domain.Entities;

namespace Kanban.Application.Boards;

public record InviteMemberRequest(string Email, BoardRole Role);
