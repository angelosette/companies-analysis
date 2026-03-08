using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Domain;
using CompaniesAnalysis.Domain.Entities;

namespace CompaniesAnalysis.Application.Funding.Strategies;

/// <summary>
/// Rules:
///  - Must have income data for ALL years 2018-2022, else $0
///  - Must have POSITIVE income in both 2021 and 2022, else $0
///  - highest income >= $10B -> 12.33%, else -> 21.51%
/// </summary>
public class StandardFundingStrategy : IFundingStrategy
{
    public decimal Calculate(Company company, IReadOnlyList<IncomeRecord> records)
    {
        var byYear = records
            .Where(r => r.Year >= FundingConstants.StartYear && r.Year <= FundingConstants.EndYear)
            .ToDictionary(r => r.Year, r => r.Value);

        var requiredYears = Enumerable.Range(
            FundingConstants.StartYear,
            FundingConstants.EndYear - FundingConstants.StartYear + 1);

        if (!requiredYears.All(y => byYear.ContainsKey(y)))
            return 0m;

        if (byYear[2021] <= 0 || byYear[2022] <= 0)
            return 0m;

        var highest = byYear.Values.Max();
        var rate = highest >= FundingConstants.HighIncomeThreshold
            ? FundingConstants.HighIncomeRate
            : FundingConstants.LowIncomeRate;

        return highest * rate;
    }
}
