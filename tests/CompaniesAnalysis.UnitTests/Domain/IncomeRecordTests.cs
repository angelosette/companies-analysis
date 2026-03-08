using CompaniesAnalysis.Domain.Entities;
using Xunit;

namespace CompaniesAnalysis.UnitTests.Domain;

public class IncomeRecordTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var record = IncomeRecord.Create(5, 2021, 1_000_000m);

        Assert.Equal(5, record.CompanyId);
        Assert.Equal(2021, record.Year);
        Assert.Equal(1_000_000m, record.Value);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void Create_WithYearOutOfRange_Throws(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IncomeRecord.Create(1, year, 0m));
    }

    [Theory]
    [InlineData(2000)]
    [InlineData(2100)]
    public void Create_AtBoundaryYears_Succeeds(int year)
    {
        var record = IncomeRecord.Create(1, year, 0m);

        Assert.Equal(year, record.Year);
    }
}
