using Microsoft.AspNetCore.Mvc;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Interfaces;

namespace MultitenantApiSingleDbSharedSchema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _tokenService = tokenService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (accessToken, refreshToken) = await _authService.LoginAsync(request.Username, request.Password);
        if (accessToken == null || refreshToken == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(new { accessToken, refreshToken });
    }
/*
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var (accessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(request.RefreshToken);
        if (accessToken == null || newRefreshToken == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        return Ok(new { accessToken, refreshToken = newRefreshToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        // Example: get the user ID from the JWT
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        await _authService.LogoutAllAsync(userId);
        return NoContent();
    }*/
}