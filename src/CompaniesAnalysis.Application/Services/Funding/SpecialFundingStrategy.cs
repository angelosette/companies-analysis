using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Domain;
using CompaniesAnalysis.Domain.Entities;

namespace CompaniesAnalysis.Application.Funding.Strategies;

/// <summary>
/// Starts equal to Standard amount, then:
///  - Company name starts with vowel -> +15% of standard
///  - 2022 income less than 2021 income -> -25% of standard
/// </summary>
public class SpecialFundingStrategy : IFundingStrategy
{
    private readonly StandardFundingStrategy _standard;

    public SpecialFundingStrategy(StandardFundingStrategy standard)
    {
        _standard = standard;
    }

    public decimal Calculate(Company company, IReadOnlyList<IncomeRecord> records)
    {
        var standardAmount = _standard.Calculate(company, records);
        if (standardAmount == 0m) return 0m;

        var special = standardAmount;

        if (company.NameStartsWithVowel())
            special += standardAmount * FundingConstants.VowelBonus;

        var income2021 = records.FirstOrDefault(r => r.Year == 2021)?.Value ?? 0m;
        var income2022 = records.FirstOrDefault(r => r.Year == 2022)?.Value ?? 0m;

        if (income2022 < income2021)
            special -= standardAmount * FundingConstants.DeclinePenalty;

        return special;
    }
}
