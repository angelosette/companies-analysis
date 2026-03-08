namespace CompaniesAnalysis.Domain.Entities;

public class Company
{
    public int Id { get; private set; }
    public int Cik { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public ICollection<IncomeRecord> IncomeRecords { get; private set; } = new List<IncomeRecord>();

    protected Company() { }

    public static Company Create(int cik, string entityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        return new Company { Cik = cik, EntityName = entityName };
    }

    public void UpdateEntityName(string entityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        EntityName = entityName;
    }

    public bool NameStartsWithVowel()
    {
        var first = EntityName.TrimStart().FirstOrDefault();
        return "AEIOUaeiou".Contains(first);
    }
}
