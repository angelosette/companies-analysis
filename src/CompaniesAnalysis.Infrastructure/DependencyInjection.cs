using CompaniesAnalysis.Domain.Interfaces;
using CompaniesAnalysis.Infrastructure.BackgroundServices;
using CompaniesAnalysis.Infrastructure.Http;
using CompaniesAnalysis.Infrastructure.Persistence;
using CompaniesAnalysis.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CompaniesAnalysis.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {    
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=companies.db"));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IIncomeRecordRepository, IncomeRecordRepository>();        

        services.AddHttpClient<ISecEdgarClient, SecEdgarClient>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.34.0");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.BaseAddress = new Uri("https://data.sec.gov/api/xbrl/companyfacts/");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddResilienceHandler("sec-edgar", builder =>
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            });
            builder.AddTimeout(TimeSpan.FromSeconds(30));
        });

        services.AddHostedService<SecImportBackgroundService>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}
