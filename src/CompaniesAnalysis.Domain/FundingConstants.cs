namespace CompaniesAnalysis.Domain;

public static class FundingConstants
{
    public const int StartYear = 2018;
    public const int EndYear = 2022;
    public const decimal HighIncomeThreshold = 10_000_000_000m;
    public const decimal HighIncomeRate = 0.1233m;
    public const decimal LowIncomeRate = 0.2151m;
    public const decimal VowelBonus = 0.15m;
    public const decimal DeclinePenalty = 0.25m;
}
