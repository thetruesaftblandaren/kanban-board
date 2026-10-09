using Kanban.Application.Common;

namespace Kanban.Application.Auth;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password);
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
    Task<string> CreateTokenAsync(Guid userId, string email);
    Task<string> CreateRefreshTokenAsync(Guid userId);
    Task<Result<RefreshResult>> RotateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
