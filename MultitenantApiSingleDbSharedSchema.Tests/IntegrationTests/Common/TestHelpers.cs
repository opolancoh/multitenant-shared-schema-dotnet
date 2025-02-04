using Microsoft.AspNetCore.Identity;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

public static class TestHelpers
{
    public static async Task CreateTestUserAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string password,
        string tenantId)
    {
        var user = new ApplicationUser
        {
            UserName = $"{username}@{tenantId}",
            TenantId = tenantId,
            DisplayName = $"Test {username}"
        };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception("Failed to create test user: " + 
                                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
