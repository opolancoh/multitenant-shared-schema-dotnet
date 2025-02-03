namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.Interfaces;

public interface ICurrentUserService
{
    string? Role { get; }
    string TenantId { get; }
}