using CompaniesAnalysis.Domain.Entities;

namespace CompaniesAnalysis.Application.Abstractions;

public interface IFundingStrategy
{
    decimal Calculate(Company company, IReadOnlyList<IncomeRecord> records);
}
