using Kanban.Application.Common;

namespace Kanban.Application.Auth;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string password);
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
    Task<string> CreateTokenAsync(Guid userId, string email);
}
