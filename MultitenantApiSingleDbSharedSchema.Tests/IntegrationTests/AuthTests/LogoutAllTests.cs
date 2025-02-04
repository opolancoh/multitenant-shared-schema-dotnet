using System.Net;
using System.Net.Http.Json;
using MultitenantApiSingleDbSharedSchema.Core.Features.Auth.DTOs;
using MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.AuthTests;

public class LogoutAllTests : BaseIntegrationTest
{
    private const string LoginUri = "/api/auth/login";
    private const string RefreshUri = "/api/auth/refresh";
    private const string LogoutAllUri = "/api/auth/logout-all";

    [Fact]
    public async Task LogoutAll_InvalidatesAllUserRefreshTokens()
    {
        // Arrange
        const string username = "John";
        const string password = "Pass123$";
        const string tenantId = "Tenant-A";

        await CreateUserAsync(username, password, tenantId);

        var loginRequest = new { Username = username, Password = password };

        // Login on two different devices
        var loginResponse1 = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens1 = await loginResponse1.Content.ReadFromJsonAsync<LoginResponse>();

        var loginResponse2 = await Client.PostAsJsonAsync(LoginUri, loginRequest);
        var tokens2 = await loginResponse2.Content.ReadFromJsonAsync<LoginResponse>();

        // Set Authorization header
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokens1!.AccessToken);

        // Act - Logout All
        var logoutAllResponse = await Client.PostAsync(LogoutAllUri, null);
        Client.DefaultRequestHeaders.Authorization = null; // Remove Authorization for further requests

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
    public async Task LogoutAll_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Act - Attempt logout-all without a token
        var response = await Client.PostAsync(LogoutAllUri, null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
