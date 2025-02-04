using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.AuthTests;

public class LoginTests : BaseIntegrationTest
{
    private const string RequestUri = "/api/auth/login";

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var request = new { Username = username, Password = password };

        // Act 
        var response = await Client.PostAsJsonAsync(RequestUri, request);
        var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(responseContent);
        Assert.False(string.IsNullOrEmpty(responseContent.AccessToken));
        Assert.False(string.IsNullOrEmpty(responseContent.RefreshToken));
    }

    [Fact]
    public async Task Login_WithInvalidUser_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";

        var request = new { Username = username, Password = password };

        // Act 
        var response = await Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var request = new { Username = username, Password = "Pass456$" };

        // Act 
        var response = await Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(null, "Pass123$")]
    [InlineData("John", null)]
    public async Task Login_WithMissingFields_ReturnsBadRequest(string? username, string? password)
    {
        // Arrange
        var request = new { Username = username, Password = password };

        // Act 
        var response = await Client.PostAsJsonAsync(RequestUri, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsValidJwtToken()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var request = new { Username = username, Password = password };

        // Act 
        var response = await Client.PostAsJsonAsync(RequestUri, request);
        var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);

        var tokenHandler = new JwtSecurityTokenHandler();
        var isValidJwt = tokenHandler.CanReadToken(responseContent.AccessToken);

        Assert.True(isValidJwt, "Access token should be a valid JWT.");
    }
}