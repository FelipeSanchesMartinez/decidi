using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class FreelancerProfileConfiguration : IEntityTypeConfiguration<FreelancerProfile>
{
    public void Configure(EntityTypeBuilder<FreelancerProfile> builder)
    {
        builder.Property(fp => fp.Title).HasMaxLength(200);
        builder.Property(fp => fp.Bio).HasMaxLength(2000);
        builder.Property(fp => fp.HourlyRate).HasPrecision(18, 2);
        builder.Property(fp => fp.PortfolioUrl).HasMaxLength(500);

        builder.HasIndex(fp => fp.UserId).IsUnique();

        builder.HasMany(fp => fp.Skills)
            .WithMany(s => s.FreelancerProfiles);
    }
}
