using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Application.DTOs;
using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompaniesAnalysis.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companies;
    private readonly IIncomeRecordRepository _income;
    private readonly ISecEdgarClient _edgar;    
    private readonly IFundingStrategyFactory _fundingStrategyFactory;
    private readonly ILogger<CompanyService> _logger;

    // Max 5 concurrent requests to SEC EDGAR to avoid rate limiting
    private static readonly SemaphoreSlim _semaphore = new(5, 5);

    public CompanyService(
        ICompanyRepository companies,
        IIncomeRecordRepository income,
        ISecEdgarClient edgar,        
        IFundingStrategyFactory fundingStrategyFactory,
        ILogger<CompanyService> logger)
    {
        _companies = companies;
        _income = income;
        _edgar = edgar;                
        _fundingStrategyFactory = fundingStrategyFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CompanyFundingDto>> GetCompaniesAsync(
        char? startsWith, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching companies, filter={Letter}", startsWith);

        var companies = await _companies.GetAllWithIncomeAsync(startsWith, ct);

        return companies.Select(c =>
        {
            var records = c.IncomeRecords.ToList();
            return new CompanyFundingDto(
                c.Id,
                c.EntityName,
                Math.Round(_fundingStrategyFactory.GetStrategy(FundingStrategyType.Standard).Calculate(c, records), 2),
                Math.Round(_fundingStrategyFactory.GetStrategy(FundingStrategyType.Special).Calculate(c, records), 2));
        }).ToList();
    }

    public async Task<ImportResult> ImportCompaniesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting import for {Count} CIKs", Ciks.Length);

        // Phase 1: Fetch all data in parallel (max 5 concurrent HTTP requests via semaphore)
        var fetchTasks = Ciks.Select(cik => FetchOneAsync(cik, ct));
        var fetched = await Task.WhenAll(fetchTasks);

        // Phase 2: Write to DB sequentially — DbContext is not thread-safe
        var errors = new List<string>();
        int imported = 0, failed = 0;

        foreach (var (cik, result, fetchError) in fetched)
        {
            if (fetchError is not null)
            {
                errors.Add(fetchError);
                failed++;
                continue;
            }

            try
            {
                await SaveOneAsync(cik, result!, ct);
                imported++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save CIK {Cik}", cik);
                errors.Add($"CIK {cik}: {ex.Message}");
                failed++;
            }
        }

        _logger.LogInformation("Import complete. Imported={I} Failed={F}", imported, failed);
        return new ImportResult(imported, failed, errors);
    }

    private async Task<(int Cik, SecEdgarFetchResult? Result, string? Error)> FetchOneAsync(
        int cik, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var result = await _edgar.FetchCompanyAsync(cik, ct);
            if (result is null)
                return (cik, null, $"CIK {cik}: no data returned");

            return (cik, result, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch CIK {Cik}", cik);
            return (cik, null, $"CIK {cik}: {ex.Message}");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveOneAsync(int cik, SecEdgarFetchResult result, CancellationToken ct)
    {    
        var existing = await _companies.GetByCikAsync(cik, ct);
        if (existing is null)
        {
            var company = Company.Create(result.Cik, result.EntityName);
            await _companies.AddAsync(company, ct);            

            var records = result.YearlyIncomeData
                .Select(r => IncomeRecord.Create(company.Id, r.Year, r.Value));
            await _income.AddRangeAsync(records, ct);
        }
        else
        {
            existing.UpdateEntityName(result.EntityName);
            await _income.DeleteByCompanyIdAsync(existing.Id, ct);

            var records = result.YearlyIncomeData
                .Select(r => IncomeRecord.Create(existing.Id, r.Year, r.Value));
            await _income.AddRangeAsync(records, ct);
        }         
    }

    public static readonly int[] Ciks =
    [
        18926, 892553, 1510524, 1858912, 1828248, 1819493, 60086, 1853630, 1761312, 1851182,
        1034665, 927628, 1125259, 1547660, 1393311, 1757143, 1958217, 312070, 310522, 1861841,
        1037868, 1696355, 1166834, 915912, 1085277, 831259, 882291, 1521036, 1824502, 1015647,
        884624, 1501103, 1397183, 1552797, 1894630, 823277, 21175, 1439124, 52827, 1730773,
        1867287, 1685428, 1007587, 92103, 1641751, 6845, 1231457, 947263, 895421, 1988979,
        1848898, 844790, 1541309, 1858007, 1729944, 726958, 1691221, 730272, 1308106, 884144,
        1108134, 1849058, 1435617, 1857518, 64803, 1912498, 1447380, 1232384, 1141788, 1549922,
        914475, 1498382, 1400897, 314808, 1323885, 1526520, 1550695, 1634293, 1756708, 1540159,
        1076691, 1980088, 1532346, 923796, 1849635, 1872292, 1227857, 1046311, 1710350, 1476150,
        1844642, 1967078, 14272, 933267, 1157557, 1560293, 217410, 1798562, 1038074, 1843370
    ];
}
