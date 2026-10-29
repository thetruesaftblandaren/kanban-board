using Kanban.Domain.Entities;

namespace Kanban.Application.Boards;

public record BoardMemberResponse(Guid UserId, string DisplayName, BoardRole Role);
