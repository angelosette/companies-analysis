using CompaniesAnalysis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompaniesAnalysis.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseAutoincrement();
        builder.Property(c => c.Cik).IsRequired();
        builder.Property(c => c.EntityName).IsRequired().HasMaxLength(500);
        builder.HasIndex(c => c.Cik).IsUnique().HasDatabaseName("IX_Companies_Cik");
        builder.HasIndex(c => c.EntityName).HasDatabaseName("IX_Companies_EntityName");
        builder.HasMany(c => c.IncomeRecords)
            .WithOne(r => r.Company)
            .HasForeignKey(r => r.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.ToTable("Companies");
    }
}
