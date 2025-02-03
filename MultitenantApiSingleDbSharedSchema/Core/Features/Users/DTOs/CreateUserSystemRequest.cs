namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;

public record CreateUserSystemRequest : CreateUserBaseRequest
{
    public string? TenantId { get; init; }
}