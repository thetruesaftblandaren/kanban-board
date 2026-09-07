namespace Kanban.Application.Auth;

public record AuthResponse(string Token, Guid UserId, string DisplayName);
