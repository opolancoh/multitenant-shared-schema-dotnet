namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;

public record LoginResponse(string? AccessToken, string? RefreshToken);