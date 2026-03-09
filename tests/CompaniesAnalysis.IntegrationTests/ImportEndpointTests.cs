using System.Net;
using System.Net.Http.Json;
using CompaniesAnalysis.Application.Services;
using CompaniesAnalysis.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace CompaniesAnalysis.IntegrationTests;

public class ImportEndpointTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public ImportEndpointTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Api-Key", "test-key");

        // Reset default: all CIKs return null unless overridden in a test
        factory.EdgarMock
            .FetchCompanyAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((SecEdgarFetchResult?)null);
    }

    [Fact]
    public async Task TriggerImport_WithoutApiKey_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsync("/api/import", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TriggerImport_WithInvalidKey_Returns403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "wrong-key");
        var response = await client.PostAsync("/api/import", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TriggerImport_WhenAllEdgarReturnNull_ReportsZeroImported()
    {
        var response = await _client.PostAsync("/api/import", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ImportSummaryDto>();
        Assert.NotNull(body);
        Assert.Equal(0, body.imported);
        Assert.True(body.failed > 0);
        Assert.NotEmpty(body.errors);
    }

    [Fact]
    public async Task TriggerImport_WhenOneEdgarSucceeds_ReturnsOkWithImportedCount()
    {
        var cik = CompanyService.Ciks[5];
        _factory.EdgarMock
            .FetchCompanyAsync(cik, Arg.Any<CancellationToken>())
            .Returns(new SecEdgarFetchResult(cik, "Imported Corp", [(2021, 5_000_000_000m)]));

        var response = await _client.PostAsync("/api/import", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ImportSummaryDto>();
        Assert.NotNull(body);
        Assert.True(body.imported >= 1);
    }

    [Fact]
    public async Task TriggerImport_ReimportExistingCompany_UpdatesRecord()
    {
        var cik = CompanyService.Ciks[6];

        // First import — creates the company (new-company code path)
        _factory.EdgarMock
            .FetchCompanyAsync(cik, Arg.Any<CancellationToken>())
            .Returns(new SecEdgarFetchResult(cik, "Original Name", [(2021, 1_000_000_000m)]));
        await _client.PostAsync("/api/import", null);

        // Second import — updates the company (existing-company code path)
        _factory.EdgarMock
            .FetchCompanyAsync(cik, Arg.Any<CancellationToken>())
            .Returns(new SecEdgarFetchResult(cik, "Updated Name", [(2021, 2_000_000_000m)]));
        var response = await _client.PostAsync("/api/import", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ImportSummaryDto>();
        Assert.NotNull(body);
        Assert.True(body.imported >= 1);
    }

    private record ImportSummaryDto(int imported, int failed, List<string> errors);
}
