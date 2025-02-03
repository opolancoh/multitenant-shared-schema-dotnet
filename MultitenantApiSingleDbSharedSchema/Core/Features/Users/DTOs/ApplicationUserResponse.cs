namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;

public record ApplicationUserResponse
{
    public Guid Id { get; init; }
    public string UserName { get; init; }
    public string DisplayName { get; init; }
    public string? Email { get; init; }
    public string TenantId { get; init; }
    public IReadOnlyList<string> Roles { get; init; }
}