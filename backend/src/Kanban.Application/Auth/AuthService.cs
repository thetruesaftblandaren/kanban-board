using Kanban.Application.Common;
using Kanban.Domain.Entities;

namespace Kanban.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;

    public AuthService(IIdentityService identityService, IUserRepository userRepository)
    {
        _identityService = identityService;
        _userRepository = userRepository;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var (succeeded, errors, userId) = await _identityService.CreateUserAsync(request.Email, request.Password);

        if (!succeeded)
            return AuthResult.Fail(errors);

        var domainUser = new User { Id = userId, DisplayName = request.DisplayName };
        await _userRepository.AddAsync(domainUser);
        await _userRepository.SaveChangesAsync();

        var token = await _identityService.CreateTokenAsync(userId, request.Email);
        return AuthResult.Ok(new AuthResponse(token, userId, domainUser.DisplayName));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var userId = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
        if (userId is null)
            return AuthResult.Fail(["Invalid email or password."]);

        var domainUser = await _userRepository.GetByIdAsync(userId.Value);
        var token = await _identityService.CreateTokenAsync(userId.Value, request.Email);

        return AuthResult.Ok(new AuthResponse(token, userId.Value, domainUser?.DisplayName ?? ""));
    }
}
