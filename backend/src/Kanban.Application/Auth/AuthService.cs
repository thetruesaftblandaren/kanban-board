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

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var createResult = await _identityService.CreateUserAsync(request.Email, request.Password);

        if (!createResult.Success)
            return Result<AuthResponse>.Fail(createResult.Errors);

        var userId = createResult.Value;

        var domainUser = new User { Id = userId, DisplayName = request.DisplayName };
        await _userRepository.AddAsync(domainUser);
        await _userRepository.SaveChangesAsync();

        var token = await _identityService.CreateTokenAsync(userId, request.Email);
        return Result<AuthResponse>.Ok(new AuthResponse(token, userId, domainUser.DisplayName));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var userId = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
        if (userId is null)
            return Result<AuthResponse>.Fail("Invalid email or password.");

        var domainUser = await _userRepository.GetByIdAsync(userId.Value);
        var token = await _identityService.CreateTokenAsync(userId.Value, request.Email);

        return Result<AuthResponse>.Ok(new AuthResponse(token, userId.Value, domainUser?.DisplayName ?? ""));
    }
}
