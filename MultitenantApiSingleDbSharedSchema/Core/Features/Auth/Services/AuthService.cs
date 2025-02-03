using Microsoft.AspNetCore.Identity;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Entities;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Interfaces;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    

    public AuthService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService
        )
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }
    
    public async Task<(string? accessToken, string? refreshToken)> LoginAsync(
        string username,
        string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return (null, null);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
            return (null, null);

        // Generate tokens
        var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Save the refresh token in the DB
        var refreshTokenExpireMinutes = _configuration.GetValue("Jwt:RefreshTokenExpireMinutes", 10080);  // default refresh token valid for 7 days
        var expiresAt = DateTime.UtcNow.AddMinutes(refreshTokenExpireMinutes);
        var refreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return (newAccessToken, newRefreshToken);
    }
    
    /* public async Task<(string? accessToken, string? refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (existingToken == null)
            return (null, null);

        // Check if expired, revoked, or user missing
        if (existingToken.IsRevoked
            || existingToken.ExpiresAt <= DateTime.UtcNow
            || existingToken.User == null)
        {
            return (null, null);
        }

        // Revoke the old token
        existingToken.IsRevoked = true;
        await _dbContext.SaveChangesAsync();

        var user = existingToken.User;
        var newAccessToken = await GenerateAccessTokenAsync(user);
        var newRefreshToken = GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return (newAccessToken, newRefreshToken);
    }
    
    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token != null && !token.IsRevoked)
        {
            token.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
        }
    }
    
    public async Task LogoutAllAsync(Guid userId)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
        }

        await _dbContext.SaveChangesAsync();
    } */
}