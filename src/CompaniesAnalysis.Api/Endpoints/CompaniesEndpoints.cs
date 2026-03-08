using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompaniesAnalysis.Api.Endpoints;

public static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/companies", GetCompanies)
            .WithName("GetCompanies")
            .WithTags("Companies")
            .WithSummary("Get companies with fundable amounts")
            .WithDescription("Returns all companies and their Standard/Special Fundable Amounts. " +
                             "Filter by first letter using the 'startsWith' query parameter.")
            .WithOpenApi();

        return app;
    }

    private static async Task<IResult> GetCompanies(
        [FromQuery] char? startsWith,
        ICompanyService service,
        ILogger<Program> logger,
        CancellationToken ct)
    {
        logger.LogInformation("GET /api/companies startsWith={S}", startsWith);
        var result = await service.GetCompaniesAsync(startsWith, ct);
        return Results.Ok(result.Select(c => new
        {
            id = c.Id,
            name = c.Name,
            standardFundableAmount = c.StandardFundableAmount,
            specialFundableAmount = c.SpecialFundableAmount
        }));
    }
}
