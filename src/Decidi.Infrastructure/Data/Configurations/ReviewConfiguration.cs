using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.Property(r => r.ClientId).IsRequired();
        builder.Property(r => r.FreelancerId).IsRequired();
        // Enums persistidos como string para legibilidade no DB e robustez em queries diretas.
        builder.Property(r => r.ReviewerRole).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(r => r.Project)
            .WithMany()
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Client)
            .WithMany()
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Freelancer)
            .WithMany()
            .HasForeignKey(r => r.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique parcial: cada lado avalia 1x por projeto.
        builder.HasIndex(r => new { r.ProjectId, r.ReviewerRole }).IsUnique();
        builder.HasIndex(r => new { r.FreelancerId, r.Visibility });
        builder.HasIndex(r => new { r.ClientId, r.Visibility });
    }
}
