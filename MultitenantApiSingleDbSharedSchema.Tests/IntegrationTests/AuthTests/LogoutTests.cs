using System.Net;
using System.Net.Http.Json;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.AuthTests;

public class LogoutTests : BaseIntegrationTest
{
    private const string LoginUri = "/api/auth/login";
    private const string RefreshUri = "/api/auth/refresh";
    private const string LogoutUri = "/api/auth/logout";

    [Fact]
    public async Task Logout_WithValidToken_RevokesRefreshToken()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var logoutRequest = new { tokens.RefreshToken };

        // Act - Logout
        var logoutResponse = await Client.PostAsJsonAsync(LogoutUri, logoutRequest);

        var refreshRequest = new { tokens.RefreshToken };
        var refreshResponse = await Client.PostAsJsonAsync(RefreshUri, refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var logoutRequest = new { RefreshToken = "invalid-refresh-token" };

        // Act
        var response = await Client.PostAsJsonAsync(LogoutUri, logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsBadRequest()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        
        var logoutRequest = new { RefreshToken = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync(LogoutUri, logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_AfterTokenAlreadyUsed_ReturnsUnauthorized()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var loginRequest = new { Username = username, Password = password };
        var loginResponse = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var logoutRequest = new { tokens.RefreshToken };

        // Act - Logout
        var firstLogoutResponse = await Client.PostAsJsonAsync(LogoutUri, logoutRequest);
        Client.DefaultRequestHeaders.Authorization = null; // Remove Authorization for further requests

        // Attempt to logout again with the same refresh token
        var secondLogoutResponse = await Client.PostAsJsonAsync(LogoutUri, logoutRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, firstLogoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondLogoutResponse.StatusCode);
    }
}