using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CompaniesAnalysis.Infrastructure.Persistence.Repositories;

public class IncomeRecordRepository : IIncomeRecordRepository
{
    private readonly AppDbContext _ctx;
    public IncomeRecordRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task AddRangeAsync(IEnumerable<IncomeRecord> records, CancellationToken ct = default)
    {
        await _ctx.IncomeRecords.AddRangeAsync(records, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task DeleteByCompanyIdAsync(int companyId, CancellationToken ct = default) 
    {
        await _ctx.IncomeRecords.Where(r => r.CompanyId == companyId).ExecuteDeleteAsync(ct);
        await _ctx.SaveChangesAsync(ct);
    }
}
