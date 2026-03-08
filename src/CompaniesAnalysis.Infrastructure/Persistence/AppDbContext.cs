using CompaniesAnalysis.Domain.Entities;
using CompaniesAnalysis.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CompaniesAnalysis.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<IncomeRecord> IncomeRecords => Set<IncomeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CompanyConfiguration());
        modelBuilder.ApplyConfiguration(new IncomeRecordConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
