using System.Security.Cryptography;
using System.Text;
using Kanban.Application.Auth;
using Kanban.Application.Common;
using Kanban.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kanban.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _context;

    public IdentityService(UserManager<ApplicationUser> userManager, TokenService tokenService, AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<Result<Guid>> CreateUserAsync(string email, string password)
    {
        var appUser = new ApplicationUser { Email = email, UserName = email };
        var result = await _userManager.CreateAsync(appUser, password);

        if (!result.Succeeded)
            return Result<Guid>.Fail(result.Errors.Select(e => e.Description));

        return Result<Guid>.Ok(appUser.Id);
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

    public async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var (entity, raw) = NewRefreshToken(userId);
        _context.RefreshTokens.Add(entity);
        await _context.SaveChangesAsync();
        return raw;
    }

    public async Task<Result<RefreshResult>> RotateRefreshTokenAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (stored is null || !stored.IsActive)
            return Result<RefreshResult>.Fail("Invalid refresh token.");
        
        var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
            return Result<RefreshResult>.Fail("Invalid refresh token.");
        
        stored.RevokedAt = DateTime.UtcNow;
        var (entity, raw) = NewRefreshToken(user.Id);
        _context.RefreshTokens.Add(entity);
        return Result<RefreshResult>.Ok(new RefreshResult(user.Id, user.Email!, raw));
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var hash = Hash(refreshToken);
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (stored is null || stored.RevokedAt is not null) return;

        stored.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static (RefreshToken entity, string raw) NewRefreshToken(Guid userId)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(raw),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        return (entity, raw);
    }

    private static string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
