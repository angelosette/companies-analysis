namespace CompaniesAnalysis.Domain.Interfaces;

public record SecEdgarFetchResult(
    int Cik,
    string EntityName,
    IReadOnlyList<(int Year, decimal Value)> YearlyIncomeData);

public interface ISecEdgarClient
{
    Task<SecEdgarFetchResult?> FetchCompanyAsync(int cik, CancellationToken ct = default);
}
