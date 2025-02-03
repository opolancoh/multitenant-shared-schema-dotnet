namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<(string? accessToken, string? refreshToken)> LoginAsync(string username, string password);
    
    /* Task<(string? accessToken, string? refreshToken)> RefreshTokenAsync(string refreshToken);
    
    Task LogoutAsync(string refreshToken);
    
    Task LogoutAllAsync(Guid userId); */
}