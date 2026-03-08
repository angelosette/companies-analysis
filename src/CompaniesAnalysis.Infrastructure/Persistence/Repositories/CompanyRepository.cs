using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CompaniesAnalysis.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _ctx;
    public CompanyRepository(AppDbContext ctx) => _ctx = ctx;

    public async Task<Company?> GetByCikAsync(int cik, CancellationToken ct = default) =>
        await _ctx.Companies.Include(c => c.IncomeRecords)
            .FirstOrDefaultAsync(c => c.Cik == cik, ct);

    public async Task<IReadOnlyList<Company>> GetAllWithIncomeAsync(
        char? startsWith = null, CancellationToken ct = default)
    {
        var query = _ctx.Companies.Include(c => c.IncomeRecords).AsQueryable();
        if (startsWith.HasValue)
        {
            var letter = startsWith.Value.ToString().ToUpper();
            query = query.Where(c => c.EntityName.ToUpper().StartsWith(letter));
        }
        return await query.OrderBy(c => c.EntityName).ToListAsync(ct);
    }

    public async Task AddAsync(Company company, CancellationToken ct = default)
    {
        await _ctx.Companies.AddAsync(company, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Company company, CancellationToken ct = default)    
    {
        _ctx.Companies.Update(company);
        await _ctx.SaveChangesAsync(ct);
    }
}
