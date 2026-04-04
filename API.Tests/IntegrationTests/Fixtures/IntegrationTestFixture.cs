using API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.IntegrationTests.Fixtures;

public class IntegrationTestFixture : IAsyncLifetime
{
    private static readonly SqliteConnection _sharedConnection = 
        new SqliteConnection("Data Source=memdb;Mode=Memory;Cache=Shared");

    public WebApplicationFactory<Program> Factory { get; private set; }

    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DataContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<DataContext>(options =>
                        options.UseSqlite(_sharedConnection));
                });

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        { "JWT:Key", "your-super-secret-key-with-at-least-64-characters-here-or-even-more-for-better-security" },
                        { "JWT:Issuer", "TestLiborrow" },
                        { "JWT:Audience", "TestLiborrowClient" }
                    });
                });
            });

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await Factory.DisposeAsync();
}