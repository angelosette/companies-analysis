using CompaniesAnalysis.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CompaniesAnalysis.Application.Funding.Strategies;

public class FundingStrategyFactory : IFundingStrategyFactory
{
    private readonly IServiceProvider _serviceProvider;

    public FundingStrategyFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFundingStrategy GetStrategy(FundingStrategyType type)
    {
        return _serviceProvider.GetRequiredKeyedService<IFundingStrategy>(type);
    }
}
