using CompaniesAnalysis.Domain.Entities;
using Xunit;

namespace CompaniesAnalysis.UnitTests.Domain;

public class CompanyTests
{
    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var company = Company.Create(42, "Test Corp");

        Assert.Equal(42, company.Cik);
        Assert.Equal("Test Corp", company.EntityName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_Throws(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => Company.Create(1, name!));
    }

    [Fact]
    public void UpdateEntityName_ChangesName()
    {
        var company = Company.Create(1, "Old Name");
        company.UpdateEntityName("New Name");

        Assert.Equal("New Name", company.EntityName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateEntityName_WithInvalidName_Throws(string? name)
    {
        var company = Company.Create(1, "Test Corp");

        Assert.ThrowsAny<ArgumentException>(() => company.UpdateEntityName(name!));
    }

    [Theory]
    [InlineData("Apple Inc", true)]
    [InlineData("Everest Corp", true)]
    [InlineData("Iron Works", true)]
    [InlineData("Orange LLC", true)]
    [InlineData("Uber Tech", true)]
    [InlineData("apple inc", true)]   // lowercase vowels
    [InlineData("Tech Corp", false)]
    [InlineData("Microsoft", false)]
    [InlineData("  Apple Inc", true)] // leading whitespace trimmed
    public void NameStartsWithVowel_ReturnsCorrectResult(string name, bool expected)
    {
        var company = Company.Create(1, name);

        Assert.Equal(expected, company.NameStartsWithVowel());
    }
}
