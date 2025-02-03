namespace MultitenantApiSingleDbSharedSchema.Core.Features.Users.DTOs;

public record CreateUserBaseRequest
{
    public string Username { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string? Role { get; init; }
}