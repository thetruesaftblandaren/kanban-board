namespace Kanban.Application.Auth;

public record AuthResponse(string Token, string RefreshToken, Guid UserId, string DisplayName);
