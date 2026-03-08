using CompaniesAnalysis.Domain.Entities;

namespace CompaniesAnalysis.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByCikAsync(int cik, CancellationToken ct = default);
    Task<IReadOnlyList<Company>> GetAllWithIncomeAsync(char? startsWith = null, CancellationToken ct = default);
    Task AddAsync(Company company, CancellationToken ct = default);
    Task UpdateAsync(Company company, CancellationToken ct = default);
}
