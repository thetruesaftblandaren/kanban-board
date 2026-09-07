namespace Kanban.Application.Auth;

public interface IIdentityService
{
    Task<(bool Succeeded, IEnumerable<string> Errors, Guid UserId)> CreateUserAsync(string email, string password);
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
    Task<string> CreateTokenAsync(Guid userId, string email);
}
