using CompaniesAnalysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompaniesAnalysis.Infrastructure.Persistence.Configurations;

public class IncomeRecordConfiguration : IEntityTypeConfiguration<IncomeRecord>
{
    public void Configure(EntityTypeBuilder<IncomeRecord> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).UseAutoincrement();
        builder.Property(r => r.Year).IsRequired();
        builder.Property(r => r.Value).IsRequired().HasPrecision(28, 4);
        builder.HasIndex(r => new { r.CompanyId, r.Year })
            .IsUnique()
            .HasDatabaseName("IX_IncomeRecords_CompanyId_Year");
        builder.ToTable("IncomeRecords");
    }
}
