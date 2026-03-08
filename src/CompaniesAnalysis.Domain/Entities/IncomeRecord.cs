namespace CompaniesAnalysis.Domain.Entities;

public class IncomeRecord
{
    public int Id { get; private set; }
    public int CompanyId { get; private set; }
    public int Year { get; private set; }
    public decimal Value { get; private set; }
    public Company Company { get; private set; } = null!;

    private IncomeRecord() { }

    public static IncomeRecord Create(int companyId, int year, decimal value)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentOutOfRangeException(nameof(year));
        return new IncomeRecord { CompanyId = companyId, Year = year, Value = value };
    }
}
