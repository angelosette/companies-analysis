using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Application.Services;

namespace CompaniesAnalysis.Api.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/import", TriggerImport)
            .WithName("TriggerImport")
            .WithTags("Import")
            .WithSummary("Manually trigger SEC EDGAR import")
            .WithDescription("Fetches and persists company income data from SEC EDGAR for all CIKs. " +
                             "Also runs automatically on startup.")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> TriggerImport(
        ICompanyService service,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Manual import triggered");
        var result = await service.ImportCompaniesAsync(ct);
        return Results.Ok(new { imported = result.Imported, failed = result.Failed, errors = result.Errors });
    }
}
