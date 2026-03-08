using CompaniesAnalysis.Application.Funding.Strategies;
using CompaniesAnalysis.Domain.Entities;
using Xunit;

namespace CompaniesAnalysis.UnitTests.Funding;

public class SpecialFundingStrategyTests
{
    private readonly StandardFundingStrategy _standard = new();
    private readonly SpecialFundingStrategy _sut;

    public SpecialFundingStrategyTests() => _sut = new(_standard);

    private static Company Company(string name) => CompaniesAnalysis.Domain.Entities.Company.Create(1, name);

    private static List<IncomeRecord> ValidRecords(decimal i2021 = 3_000_000_000m, decimal i2022 = 3_500_000_000m) =>
    [
        IncomeRecord.Create(1, 2018, 1_000_000_000m),
        IncomeRecord.Create(1, 2019, 2_000_000_000m),
        IncomeRecord.Create(1, 2020, 500_000_000m),
        IncomeRecord.Create(1, 2021, i2021),
        IncomeRecord.Create(1, 2022, i2022),
    ];

    [Fact]
    public void WhenVowelName_Adds15Percent()
    {
        var company = Company("Apple Inc.");
        var records = ValidRecords();
        var std = _standard.Calculate(company, records);

        Assert.Equal(std * 1.15m, _sut.Calculate(company, records));
    }

    [Fact]
    public void WhenConsonantName_And2022Greater_SameAsStandard()
    {
        var company = Company("Tech Corp");
        var records = ValidRecords(3_000_000_000m, 3_500_000_000m);
        var std = _standard.Calculate(company, records);

        Assert.Equal(std, _sut.Calculate(company, records));
    }

    [Fact]
    public void When2022LessThan2021_Subtracts25Percent()
    {
        var company = Company("Tech Corp");
        var records = ValidRecords(i2021: 3_000_000_000m, i2022: 2_000_000_000m);
        var std = _standard.Calculate(company, records);

        Assert.Equal(std * 0.75m, _sut.Calculate(company, records));
    }

    [Fact]
    public void WhenVowelNameAnd2022Less_AppliesBothAdjustments()
    {
        var company = Company("Uber Technologies");
        var records = ValidRecords(i2021: 3_000_000_000m, i2022: 1_000_000_000m);
        var std = _standard.Calculate(company, records);

        var expected = std + (std * 0.15m) - (std * 0.25m);
        Assert.Equal(expected, _sut.Calculate(company, records));
    }

    [Fact]
    public void WhenStandardIsZero_ReturnsZero()
    {
        var company = Company("Any Corp");
        // Only one year = standard will be 0
        var records = new List<IncomeRecord> { IncomeRecord.Create(1, 2021, 1_000_000m) };

        Assert.Equal(0m, _sut.Calculate(company, records));
    }

    [Theory]
    [InlineData("Apple Corp")]
    [InlineData("Everest Inc")]
    [InlineData("Iron Works")]
    [InlineData("Orange LLC")]
    [InlineData("United Corp")]
    public void VowelDetection_WorksForAllVowels(string name)
    {
        var company = Company(name);
        var records = ValidRecords();
        var std = _standard.Calculate(company, records);

        Assert.True(_sut.Calculate(company, records) > std);
    }
}
