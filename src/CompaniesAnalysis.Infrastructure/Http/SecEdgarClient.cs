using System.Net.Http.Json;
using System.Text.Json;
using CompaniesAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CompaniesAnalysis.Infrastructure.Http;

public class SecEdgarClient : ISecEdgarClient
{
    private readonly HttpClient _http;
    private readonly ILogger<SecEdgarClient> _logger;    

    // Matches exactly CY + 4 digits (e.g. CY2021), not CY2021Q1
    private static bool IsAnnualFrame(string? frame) =>
        frame is { Length: 6 } &&
        frame.StartsWith("CY", StringComparison.OrdinalIgnoreCase) &&
        int.TryParse(frame.AsSpan(2), out _);

    public SecEdgarClient(HttpClient http, ILogger<SecEdgarClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SecEdgarFetchResult?> FetchCompanyAsync(int cik, CancellationToken ct = default)
    {
        var paddedCik = cik.ToString().PadLeft(10, '0');
        var url = $"CIK{paddedCik}.json";

        _logger.LogDebug("SEC EDGAR request: {Url}", url);

        HttpResponseMessage response;
        try
        {
            response = await _http.GetAsync(url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP failure for CIK {Cik}", cik);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("SEC returned {Status} for CIK {Cik}", response.StatusCode, cik);
            return null;
        }

        EdgarCompanyInfo? info;
        try
        {
            info = await response.Content.ReadFromJsonAsync<EdgarCompanyInfo>(cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Deserialization failed for CIK {Cik}", cik);
            return null;
        }

        if (info is null) return null;

        var usdRecords = info.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd ?? [];

        var yearlyIncome = usdRecords
            .Where(r => r.Form == "10-K" && IsAnnualFrame(r.Frame))
            .GroupBy(r => int.Parse(r.Frame![2..]))
            .Select(g => g.OrderByDescending(r => r.Filed).First())
            .Select(r => (Year: int.Parse(r.Frame![2..]), Value: r.Val))
            .ToList();

        _logger.LogDebug("CIK {Cik} ({Name}) has {Count} annual 10-K records", cik, info.EntityName, yearlyIncome.Count);

        return new SecEdgarFetchResult(info.Cik, info.EntityName, yearlyIncome);
    }
}
