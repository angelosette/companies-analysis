using CompaniesAnalysis.Application.DTOs;

namespace CompaniesAnalysis.Application.Abstractions;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyFundingDto>> GetCompaniesAsync(char? startsWith, CancellationToken ct = default);
    Task<ImportResult> ImportCompaniesAsync(CancellationToken ct = default);
}
