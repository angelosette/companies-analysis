using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Application.Funding.Strategies;
using CompaniesAnalysis.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CompaniesAnalysis.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<StandardFundingStrategy>();
        services.AddKeyedScoped<IFundingStrategy, StandardFundingStrategy>(FundingStrategyType.Standard);
        services.AddKeyedScoped<IFundingStrategy, SpecialFundingStrategy>(FundingStrategyType.Special);
        services.AddScoped<IFundingStrategyFactory, FundingStrategyFactory>();

        services.AddScoped<ICompanyService, CompanyService>();
        return services;
    }
}
