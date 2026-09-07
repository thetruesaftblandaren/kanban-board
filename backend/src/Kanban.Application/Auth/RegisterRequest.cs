namespace Kanban.Application.Auth;

public record RegisterRequest(string Email, string Password, string DisplayName);
