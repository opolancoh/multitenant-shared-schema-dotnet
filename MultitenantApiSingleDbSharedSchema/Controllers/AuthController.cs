using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.Interfaces;

namespace MultitenantApiSingleDbSharedSchema.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (accessToken, refreshToken) = await _authService.LoginAsync(request.Username, request.Password);
        if (accessToken == null || refreshToken == null)
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        return Ok(new { accessToken, refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var (accessToken, newRefreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (accessToken == null || newRefreshToken == null)
        {
            return Unauthorized(new { Message = "Invalid or expired refresh token." });
        }

        return Ok(new { accessToken, refreshToken = newRefreshToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken);

        if (!result.Succeeded)
            return BadRequest(new { Message = "One or more validation errors occurred.", result.Errors });

        return Ok(new { Message = "Logged out successfully." });
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        var result = await _authService.LogoutAllAsync();
        
        if (!result.Succeeded)
            return BadRequest(new { Message = "Logout failed. Please try again."});

        return NoContent();
    }
}