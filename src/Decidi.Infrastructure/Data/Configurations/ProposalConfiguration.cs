using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.CoverLetter).HasMaxLength(3000).IsRequired();

        builder.HasOne(p => p.Freelancer)
            .WithMany(u => u.Proposals)
            .HasForeignKey(p => p.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Project)
            .WithMany(proj => proj.Proposals)
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.FreelancerId, p.ProjectId }).IsUnique();
        builder.HasIndex(p => new { p.ProjectId, p.Status });
        // Minhas Propostas (freelancer) filtrando por status.
        builder.HasIndex(p => new { p.FreelancerId, p.Status });

        builder.Property(p => p.RowVersion).IsConcurrencyToken();
    }
}
