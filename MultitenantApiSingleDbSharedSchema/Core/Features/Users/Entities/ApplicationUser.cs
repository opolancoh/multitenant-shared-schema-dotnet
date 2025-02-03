using Microsoft.AspNetCore.Identity;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Entities;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string TenantId { get; set; }
    
    public string DisplayName { get; set; }
    
    // Optional: a navigation property if you want to track all refresh tokens for the user
    public virtual List<RefreshToken> RefreshTokens { get; set; } = [];
}