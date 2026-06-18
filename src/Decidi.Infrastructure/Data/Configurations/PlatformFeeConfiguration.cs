using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class PlatformFeeConfiguration : IEntityTypeConfiguration<PlatformFee>
{
    public void Configure(EntityTypeBuilder<PlatformFee> builder)
    {
        builder.Property(p => p.ClientFee).HasPrecision(18, 2);
        builder.Property(p => p.FreelancerFee).HasPrecision(18, 2);
        builder.Property(p => p.CommissionPct).HasPrecision(5, 2);
        builder.Property(p => p.Note).HasMaxLength(500);

        builder.HasIndex(p => p.EffectiveFrom);
        builder.HasIndex(p => p.IsActive);
    }
}
