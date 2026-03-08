using CompaniesAnalysis.Domain.Entities;

namespace CompaniesAnalysis.Domain.Interfaces;

public interface IIncomeRecordRepository
{
    Task AddRangeAsync(IEnumerable<IncomeRecord> records, CancellationToken ct = default);
    Task DeleteByCompanyIdAsync(int companyId, CancellationToken ct = default);
}
