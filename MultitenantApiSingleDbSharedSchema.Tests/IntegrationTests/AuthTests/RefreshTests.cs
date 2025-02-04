using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.AuthTests;

public class RefreshTokenTests : BaseIntegrationTest
{
    private const string LoginUri = "/api/auth/login";
    private const string RefreshUri = "/api/auth/refresh";
    private const string LogoutUri = "/api/auth/logout";
    private const string LogoutAllUri = "/api/auth/logout-all";

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new { tokens!.RefreshToken };

        // Act
        var response = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);
        var newTokens = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(newTokens);
        Assert.False(string.IsNullOrEmpty(newTokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(newTokens.RefreshToken));
    }

    [Fact]
    public async Task Refresh_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);
        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Simulate token expiration
        await ExpireRefreshTokenAsync(tokens!.RefreshToken);

        var refreshRequest = new { tokens.RefreshToken };

        // Act
        var response = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithUsedToken_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);
        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new { tokens!.RefreshToken };

        // First refresh
        var firstResponse = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Second refresh attempt with the same token
        var secondResponse = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new { RefreshToken = "invalid-refresh-token" };

        // Act
        var response = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterLogout_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        // Login
        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        // Logout
        var logoutRequest = new { tokens.RefreshToken };
        var logoutResponse = await Client.PostAsJsonAsync(LogoutUri, logoutRequest);

        // Act
        var refreshRequest = new { tokens.RefreshToken };
        var refreshResponse = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }


    [Fact]
    public async Task Refresh_AfterLogoutAll_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);
        var loginRequest = new { Username = username, Password = password };
        var loginResponse1 = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens1 = await loginResponse1.Content.ReadFromJsonAsync<LoginResponse>();

        var loginResponse2 = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens2 = await loginResponse2.Content.ReadFromJsonAsync<LoginResponse>();

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens1!.AccessToken);

        // Act
        var logoutAllResponse = await Client.PostAsync(LogoutAllUri, null);

        var refreshRequest1 = new { tokens1.RefreshToken };
        var refreshRequest2 = new { tokens2!.RefreshToken };

        var refreshResponse1 = await Client.PostAsJsonAsync(RefreshUri, refreshRequest1);
        var refreshResponse2 = await Client.PostAsJsonAsync(RefreshUri, refreshRequest2);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, logoutAllResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse1.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse2.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_ReturnsValidJwtToken()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var refreshRequest = new { tokens!.RefreshToken };

        // Act
        var response = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);
        var newTokens = await response.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(newTokens);

        var tokenHandler = new JwtSecurityTokenHandler();
        var isValidJwt = tokenHandler.CanReadToken(newTokens.AccessToken);

        Assert.True(isValidJwt, "Access token should be a valid JWT.");
    }
}