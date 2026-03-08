using CompaniesAnalysis.Application.Funding.Strategies;
using CompaniesAnalysis.Domain.Entities;
using Xunit;

namespace CompaniesAnalysis.UnitTests.Funding;

public class StandardFundingStrategyTests
{
    private readonly StandardFundingStrategy _sut = new();

    private static Company Company(string name = "Test Corp") =>
        CompaniesAnalysis.Domain.Entities.Company.Create(1, name);

    private static List<IncomeRecord> Records(Dictionary<int, decimal> data) =>
        data.Select(kv => IncomeRecord.Create(1, kv.Key, kv.Value)).ToList();

    [Fact]
    public void WhenAllYearsPresent_And2021And2022Positive_ReturnsCorrectAmount()
    {
        var records = Records(new() {
            [2018] = 1_000_000_000m, [2019] = 2_000_000_000m,
            [2020] = 500_000_000m,   [2021] = 3_000_000_000m, [2022] = 2_500_000_000m
        });

        var result = _sut.Calculate(Company(), records);

        // Highest = 3B < 10B -> 21.51%
        Assert.Equal(3_000_000_000m * 0.2151m, result);
    }

    [Fact]
    public void WhenHighestIncomeOver10B_UsesHighRate()
    {
        var records = Records(new() {
            [2018] = 15_000_000_000m, [2019] = 5_000_000_000m,
            [2020] = 6_000_000_000m,  [2021] = 7_000_000_000m, [2022] = 8_000_000_000m
        });

        var result = _sut.Calculate(Company(), records);

        // Highest = 15B >= 10B -> 12.33%
        Assert.Equal(15_000_000_000m * 0.1233m, result);
    }

    [Fact]
    public void WhenMissingYear_ReturnsZero()
    {
        var records = Records(new() {
            [2018] = 1_000_000m, [2019] = 2_000_000m,
            // missing 2020
            [2021] = 3_000_000m, [2022] = 4_000_000m
        });

        Assert.Equal(0m, _sut.Calculate(Company(), records));
    }

    [Fact]
    public void When2021Negative_ReturnsZero()
    {
        var records = Records(new() {
            [2018] = 1_000_000m, [2019] = 2_000_000m,
            [2020] = 3_000_000m, [2021] = -500_000m, [2022] = 4_000_000m
        });

        Assert.Equal(0m, _sut.Calculate(Company(), records));
    }

    [Fact]
    public void When2022Zero_ReturnsZero()
    {
        var records = Records(new() {
            [2018] = 1_000_000m, [2019] = 2_000_000m,
            [2020] = 3_000_000m, [2021] = 4_000_000m, [2022] = 0m
        });

        Assert.Equal(0m, _sut.Calculate(Company(), records));
    }

    [Fact]
    public void WithEmptyRecords_ReturnsZero() =>
        Assert.Equal(0m, _sut.Calculate(Company(), []));
}
