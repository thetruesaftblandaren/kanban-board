namespace Kanban.Application.Auth;

public record RefreshResult(Guid UserId, string Email, string NewRefreshToken);
