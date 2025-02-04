using MultitenantApiSingleDbSharedSchema.Core.Common;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<LoginResponse>  LoginAsync(string username, string password);
    
    Task<RefreshTokenResponse>  RefreshTokenAsync(string refreshToken);
    
    Task<OperationResult> LogoutAsync(string refreshToken);
    
    Task<OperationResult> LogoutAllAsync();
}