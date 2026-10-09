using Kanban.Application.Common;

namespace Kanban.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request);
    Task LogoutAsync(RefreshRequest request);
}
