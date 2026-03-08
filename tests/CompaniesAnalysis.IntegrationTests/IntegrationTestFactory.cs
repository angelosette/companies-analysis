using CompaniesAnalysis.Api.Middleware;
using CompaniesAnalysis.Domain.Interfaces;
using CompaniesAnalysis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace CompaniesAnalysis.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Keep a single open connection so the in-memory SQLite database
    // is not dropped between requests.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ISecEdgarClient EdgarMock { get; } = Substitute.For<ISecEdgarClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlite(_connection));

            // Suppress background import so tests start with a clean DB
            services.RemoveAll<IHostedService>();

            // Replace the real SEC EDGAR client with a mock
            services.RemoveAll<ISecEdgarClient>();
            services.AddSingleton(EdgarMock);

            services.Configure<ApiKeyConfigOptions>(opts =>
                opts.ValidKeys = ["test-key"]);
        });
        builder.UseEnvironment("Test");
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
