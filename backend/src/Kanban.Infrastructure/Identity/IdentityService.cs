using Kanban.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace Kanban.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public IdentityService(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<(bool, IEnumerable<string>, Guid)> CreateUserAsync(string email, string password)
    {
        var appUser = new ApplicationUser { Email = email, UserName = email };
        var result = await _userManager.CreateAsync(appUser, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description), appUser.Id);
    }

    public async Task<Guid?> ValidateCredentialsAsync(string email, string password)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser is null) return null;

        var valid = await _userManager.CheckPasswordAsync(appUser, password);
        return valid ? appUser.Id : null;
    }

    public Task<string> CreateTokenAsync(Guid userId, string email)
    {
        return Task.FromResult(_tokenService.CreateToken(userId, email));
    }
}
