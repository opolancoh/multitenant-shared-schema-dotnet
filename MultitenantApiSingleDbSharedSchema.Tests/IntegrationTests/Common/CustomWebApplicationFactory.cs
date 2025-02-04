using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultitenantApiSingleDbSharedSchema.Infrastructure.Persistence;

namespace MultitenantApiSingleDbSharedSchema.Tests.IntegrationTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _testDbName;

    public CustomWebApplicationFactory()
    {
        // Create a random DB name so each instance has a unique DB
        _testDbName = $"multitenant_api_{Guid.NewGuid():N}";
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // 1. Load appsettings.Test.json
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
        });

        builder.ConfigureServices(services =>
        {
            // 2. Remove existing ApplicationDbContext registration (if any)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 3. Build an intermediate service provider so we can read config
            var sp = services.BuildServiceProvider();
            var configuration = sp.GetRequiredService<IConfiguration>();

            // 4. Get the template from appsettings
            var template = configuration.GetConnectionString("TestDb");
            // e.g. "Host=localhost;Database=##DBNAME##;Username=postgres;Password=postgres"

            // 5. Replace the placeholder with the random DB name
            var finalConnectionString = template.Replace("##DBNAME##", _testDbName);

            // 6. Register our DbContext with the final connection string
            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseNpgsql(finalConnectionString);
            });
        });

        // 7. Build the host normally
        var host = base.CreateHost(builder);

        // 8. Ensure the DB is created
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        // dbContext.Database.Migrate();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        // Optionally drop the DB after tests for cleanup
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                dbContext.Database.EnsureDeleted();
            }
            catch
            {
                // swallow or log
            }
        }

        base.Dispose(disposing);
    }
}
