using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultitenantApiSingleDbSharedSchema.Core.Features.Users.Entities;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly string DatabaseName;

    protected BaseIntegrationTest()
    {
        DatabaseName = $"multitenant_api_test_{Guid.NewGuid():N}";

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException(
                                       "Database connection string not found in appsettings.json");

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContextOptions
                    var descriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    var testConnectionString = $"{baseConnectionString};Database={DatabaseName}";
                    services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(testConnectionString));
                });
            });

        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected async Task<ApplicationUser?> CreateUserAsync(
        string username,
        string password,
        string tenantId,
        string? displayName = null)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (string.IsNullOrEmpty(displayName))
            displayName = username;
        var user = new ApplicationUser { UserName = username, DisplayName = displayName, TenantId = tenantId };
        var result = await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new System.Exception($"Failed to create user: {errors}");
        }

        return user;
    }
    
    protected async Task ExpireRefreshTokenAsync(string refreshToken)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var token = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token != null)
        {
            token.ExpiresAt = DateTime.UtcNow.AddMinutes(-1); // Set expiration to the past
            await dbContext.SaveChangesAsync();
        }
    }
}
