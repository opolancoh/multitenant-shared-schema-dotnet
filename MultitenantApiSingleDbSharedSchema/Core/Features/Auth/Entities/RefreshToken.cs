using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;

namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Guid UserId { get; set; } // FK to ApplicationUser
    public ApplicationUser? User { get; set; } // Navigation
}