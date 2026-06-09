using KycAggregationService.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace KycAggregationService.Api.Persistence;

public class KycAggregationDbContext(DbContextOptions<KycAggregationDbContext> options) : DbContext(options)
{
    public DbSet<AggregatedKycDataEntity> AggregatedKycData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggregatedKycDataEntity>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Ssn).IsUnique();

            entity.Property(x => x.Ssn).IsRequired();

            entity.Property(x => x.FirstName).IsRequired();

            entity.Property(x => x.LastName).IsRequired();

            entity.Property(x => x.Address).IsRequired();

            entity.Property(x => x.TaxCountry).IsRequired();
        });
    }
}