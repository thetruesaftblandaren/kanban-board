namespace Kanban.Application.Auth;

public class AuthResult
{
    public bool Success { get; init; }
    public AuthResponse? Response { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];

    public static AuthResult Ok(AuthResponse response) => new() { Success = true, Response = response };
    public static AuthResult Fail(IEnumerable<string> errors) => new() { Success = false, Errors = errors };
}
