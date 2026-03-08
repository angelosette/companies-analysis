namespace CompaniesAnalysis.Domain.ValueObjects;

public record FundingResult(decimal StandardFundableAmount, decimal SpecialFundableAmount)
{
    public static FundingResult Zero => new(0m, 0m);
}
