namespace CompaniesAnalysis.Application.Abstractions;

public interface IFundingStrategyFactory
{
    IFundingStrategy GetStrategy(FundingStrategyType type);
}