using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;

namespace MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedDefaultDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Define default roles
        var defaultRoles = new[] { "System", "Admin" };
        foreach (var role in defaultRoles)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;

            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // Define default users
        var defaultUsers =
            new List<(string TenantId, string UserName, string Password, string DisplayName, string Role)>
            {
                ("system", "system@system", "System123+", "System", "System"),
                ("tenant-001", "admin@tenant-001", "Admin123+t1", "Admin", "Admin"),
                ("tenant-002", "admin@tenant-002", "Admin123+t2", "Admin", "Admin"),
                ("tenant-001", "user1@tenant-001", "User123+t1", "User 1", ""),
                ("tenant-002", "user1@tenant-002", "User123+t2", "User 1", "")
            };

        // Process each user
        foreach (var (tenantId, userName, password, displayName, role) in defaultUsers)
        {
            if (await userManager.Users.AnyAsync(u => u.UserName == userName)) continue;

            var newUser = new ApplicationUser
            {
                UserName = userName,
                DisplayName = displayName,
                TenantId = tenantId
            };

            var createUserResult = await userManager.CreateAsync(newUser, password);
            if (!createUserResult.Succeeded)
            {
                continue;
            }

            // Try adding the user to a role if one is specified
            if (!string.IsNullOrWhiteSpace(role))
            {
                await userManager.AddToRoleAsync(newUser, role);
            }
        }
    }
}