using CompaniesAnalysis.Application.Abstractions;
using CompaniesAnalysis.Application.Services;
using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CompaniesAnalysis.UnitTests.Import;

public class CompanyServiceImportTests
{
    private readonly ISecEdgarClient _edgar = Substitute.For<ISecEdgarClient>();
    private readonly ICompanyRepository _companies = Substitute.For<ICompanyRepository>();
    private readonly IIncomeRecordRepository _income = Substitute.For<IIncomeRecordRepository>();
    private readonly IFundingStrategyFactory _fundingStrategyFactory = Substitute.For<IFundingStrategyFactory>();
    private readonly CompanyService _sut;

    public CompanyServiceImportTests()
    {
        _sut = new CompanyService(
            _companies, _income, _edgar,
            _fundingStrategyFactory,
            NullLogger<CompanyService>.Instance);
    }

    [Fact]
    public async Task WhenAllCiksSucceed_ReturnsCorrectCount()
    {
        foreach (var cik in CompanyService.Ciks.Take(3))
        {
            _edgar.FetchCompanyAsync(cik, Arg.Any<CancellationToken>())
                .Returns(new SecEdgarFetchResult(cik, $"Corp {cik}", []));
            _companies.GetByCikAsync(cik, Arg.Any<CancellationToken>())
                .Returns((Company?)null);
        }

        var result = await _sut.ImportCompaniesAsync();

        // 3 stubbed CIKs succeed; remaining 97 get null and count as failed
        Assert.Equal(3, result.Imported);
    }

    [Fact]
    public async Task WhenEdgarReturnsNull_CountsAsFailed()
    {
        // All CIKs will return null since none are stubbed
        var result = await _sut.ImportCompaniesAsync();

        Assert.Equal(0, result.Imported);
        Assert.True(result.Failed > 0);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task WhenEdgarThrows_CountsAsFailedAndContinues()
    {
        _edgar.FetchCompanyAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("timeout"));

        var result = await _sut.ImportCompaniesAsync();

        Assert.Equal(0, result.Imported);
        Assert.True(result.Failed > 0);
    }

    [Fact]
    public async Task WhenCompanyExists_DeletesAndReAddsIncomeRecords()
    {
        var existing = Company.Create(18926, "Old Name");

        _edgar.FetchCompanyAsync(18926, Arg.Any<CancellationToken>())
            .Returns(new SecEdgarFetchResult(18926, "New Name", [(2021, 1_000_000m)]));
        _companies.GetByCikAsync(18926, Arg.Any<CancellationToken>())
            .Returns(existing);

        await _sut.ImportCompaniesAsync();

        await _income.Received(1).DeleteByCompanyIdAsync(existing.Id, Arg.Any<CancellationToken>());
        await _income.Received(1).AddRangeAsync(Arg.Any<IEnumerable<IncomeRecord>>(), Arg.Any<CancellationToken>());
    }
}
