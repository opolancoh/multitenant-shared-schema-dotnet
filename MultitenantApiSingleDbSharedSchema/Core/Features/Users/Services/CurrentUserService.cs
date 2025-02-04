using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim =
                _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ?? string.Empty;

            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public string Role =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public string TenantId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value ?? string.Empty;
}