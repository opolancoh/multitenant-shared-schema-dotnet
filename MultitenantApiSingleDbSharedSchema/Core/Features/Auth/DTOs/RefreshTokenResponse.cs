namespace MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;

public record RefreshTokenResponse(string? AccessToken, string? RefreshToken);