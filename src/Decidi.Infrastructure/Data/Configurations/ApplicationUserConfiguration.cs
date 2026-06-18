using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Role).IsRequired();
        builder.Property(u => u.City).HasMaxLength(120);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.TermsVersion).HasMaxLength(20);

        builder.HasOne(u => u.FreelancerProfile)
            .WithOne(fp => fp.User)
            .HasForeignKey<FreelancerProfile>(fp => fp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
