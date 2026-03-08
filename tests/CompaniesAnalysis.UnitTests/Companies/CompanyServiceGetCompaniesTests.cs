using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Application.Services;
using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace CompaniesAnalysis.UnitTests.Companies;

public class CompanyServiceGetCompaniesTests
{
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
    private readonly IIncomeRecordRepository _income = Substitute.For<IIncomeRecordRepository>();
    private readonly ISecEdgarClient _edgar = Substitute.For<ISecEdgarClient>();
    private readonly IFundingStrategyFactory _factory = Substitute.For<IFundingStrategyFactory>();
    private readonly IFundingStrategy _standardStrategy = Substitute.For<IFundingStrategy>();
    private readonly IFundingStrategy _specialStrategy = Substitute.For<IFundingStrategy>();
    private readonly CompanyService _sut;

    public CompanyServiceGetCompaniesTests()
    {
        _factory.GetStrategy(FundingStrategyType.Standard).Returns(_standardStrategy);
        _factory.GetStrategy(FundingStrategyType.Special).Returns(_specialStrategy);
        _sut = new CompanyService(_companies, _income, _edgar, _factory, NullLogger<CompanyService>.Instance);
    }

    [Fact]
    public async Task WhenNoCompanies_ReturnsEmptyList()
    {
        _companies.GetAllWithIncomeAsync(null, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _sut.GetCompaniesAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task WhenCompaniesExist_MapsToDtoWithRoundedAmounts()
    {
        var company = Company.Create(42, "Test Corp");
        _companies.GetAllWithIncomeAsync(null, Arg.Any<CancellationToken>())
            .Returns([company]);
        _standardStrategy.Calculate(company, Arg.Any<IReadOnlyList<IncomeRecord>>())
            .Returns(1_234_567.891m);
        _specialStrategy.Calculate(company, Arg.Any<IReadOnlyList<IncomeRecord>>())
            .Returns(2_345_678.126m);

        var result = await _sut.GetCompaniesAsync(null);

        Assert.Single(result);
        var dto = result[0];
        Assert.Equal("Test Corp", dto.Name);
        Assert.Equal(1_234_567.89m, dto.StandardFundableAmount);  // rounded to 2dp
        Assert.Equal(2_345_678.13m, dto.SpecialFundableAmount);   // rounded to 2dp
    }

    [Fact]
    public async Task PassesStartsWithFilter_ToRepository()
    {
        _companies.GetAllWithIncomeAsync('A', Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.GetCompaniesAsync('A');

        await _companies.Received(1).GetAllWithIncomeAsync('A', Arg.Any<CancellationToken>());
    }
}
