using CompaniesAnalysis.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompaniesAnalysis.Infrastructure.BackgroundServices;

public class SecImportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SecImportBackgroundService> _logger;

    public SecImportBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SecImportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for DB migrations to complete
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);                

        _logger.LogInformation("Running startup SEC import");
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICompanyService>();

        try
        {
            var result = await service.ImportCompaniesAsync(stoppingToken);
            _logger.LogInformation("Startup import done. Imported={Imported} Failed={Failed}", result.Imported, result.Failed);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Startup import cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup import failed");
        }
    }
}
