using System.Net;
using System.Net.Http.Json;
using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CompaniesAnalysis.IntegrationTests;

public class CompaniesEndpointTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;

    public CompaniesEndpointTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Api-Key", "test-key");
    }

    [Fact]
    public async Task GetCompanies_WithoutApiKey_Returns401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/companies");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanies_WithWrongKey_Returns403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "bad-key");
        var response = await client.GetAsync("/api/companies");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanies_WithValidKey_Returns200()
    {
        var response = await _client.GetAsync("/api/companies");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCompanies_ReturnsCorrectShape()
    {
        await Seed("Full Corp", 88801, 1e9m, 2e9m, 3e9m, 4e9m, 5e9m);

        var response = await _client.GetAsync("/api/companies");
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();

        Assert.NotNull(companies);
        var c = companies.FirstOrDefault(x => x.Name == "Full Corp");
        Assert.NotNull(c);
        Assert.True(c.Id > 0);
        Assert.True(c.StandardFundableAmount > 0);
        Assert.True(c.SpecialFundableAmount > 0);
    }

    [Fact]
    public async Task GetCompanies_StartsWithFilter_ReturnsOnlyMatches()
    {
        await Seed("Alpha Test", 88802, 1e9m, 2e9m, 3e9m, 4e9m, 5e9m);
        await Seed("Beta Test", 88803, 1e9m, 2e9m, 3e9m, 4e9m, 5e9m);

        var response = await _client.GetAsync("/api/companies?startsWith=A");
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();

        Assert.NotNull(companies);
        Assert.All(companies, c =>
            Assert.StartsWith("A", c.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetCompanies_MissingYear_FundableAmountIsZero()
    {
        await Seed("Partial Corp", 88804, 1e9m, null, 3e9m, 4e9m, 5e9m);

        var response = await _client.GetAsync("/api/companies");
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();

        Assert.NotNull(companies);
        var c = companies.FirstOrDefault(x => x.Name == "Partial Corp");
        Assert.NotNull(c);
        Assert.Equal(0m, c.StandardFundableAmount);
        Assert.Equal(0m, c.SpecialFundableAmount);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task Seed(string name, int cik,
        decimal y2018, decimal? y2019, decimal y2020, decimal y2021, decimal y2022)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Companies.Any(c => c.Cik == cik)) return;

        var company = Company.Create(cik, name);
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var records = new List<IncomeRecord>
        {
            IncomeRecord.Create(company.Id, 2018, y2018),
            IncomeRecord.Create(company.Id, 2020, y2020),
            IncomeRecord.Create(company.Id, 2021, y2021),
            IncomeRecord.Create(company.Id, 2022, y2022),
        };
        if (y2019.HasValue) records.Add(IncomeRecord.Create(company.Id, 2019, y2019.Value));

        db.IncomeRecords.AddRange(records);
        await db.SaveChangesAsync();
    }

    private record CompanyDto(int Id, string Name, decimal StandardFundableAmount, decimal SpecialFundableAmount);
}
