namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? Role { get; }
    string TenantId { get; }
}