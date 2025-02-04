using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultitenantApiSingleDbSharedSchema.Core.Common;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Entities;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Interfaces;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService,
        ICurrentUserService currentUserService
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return new LoginResponse(null, null);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
            return new LoginResponse(null, null);

        var accessToken = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, refreshToken);

        return new LoginResponse(accessToken, refreshToken);
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (existingToken == null || existingToken.IsRevoked || existingToken.ExpiresAt <= DateTime.UtcNow ||
            existingToken.User == null)
            return new RefreshTokenResponse(null, null);

        var user = existingToken.User;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Revoke old token
            existingToken.IsRevoked = true;
            await _dbContext.SaveChangesAsync();

            var newAccessToken = await _tokenService.GenerateAccessTokenAsync(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await SaveRefreshTokenAsync(user.Id, newRefreshToken);

            await transaction.CommitAsync();
            return new RefreshTokenResponse(newAccessToken, refreshToken);
        }
        catch
        {
            await transaction.RollbackAsync();
            return new RefreshTokenResponse(null, null);
        }
    }

    public async Task<OperationResult> LogoutAsync(string refreshToken)
    {
        // Sign out from Identity
        await _signInManager.SignOutAsync();

        // Invalidate the current refresh token
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null)
            return OperationResult.Failure([
                new OperationResultError() { Code = "RefreshToken", Description = "Invalid or expired refresh token." }
            ]);

        token.IsRevoked = true;
        await _dbContext.SaveChangesAsync();

        return OperationResult.Success();
    }

    public async Task<OperationResult> LogoutAllAsync()
    {
        var currentUserId = _currentUserService.UserId;

        await _signInManager.SignOutAsync();

        var revokeRefreshTokensResult = await _dbContext.RefreshTokens
            .Where(t => t.UserId == currentUserId && !t.IsRevoked)
            .ExecuteUpdateAsync(t => t.SetProperty(p => p.IsRevoked, true));

        if (revokeRefreshTokensResult == 0)
            return OperationResult.Failure([
                new OperationResultError { Code = "_", Description = "Unable to revoke refresh tokens." }
            ]);

        return OperationResult.Success();
    }

    private async Task SaveRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenService.GetRefreshTokenExpiryMinutes()),
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();
    }
}