using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.GrossAmount).HasPrecision(18, 2);
        builder.Property(p => p.ClientFee).HasPrecision(18, 2);
        builder.Property(p => p.FreelancerFee).HasPrecision(18, 2);
        builder.Property(p => p.CommissionPct).HasPrecision(5, 2);
        builder.Property(p => p.CommissionAmount).HasPrecision(18, 2);
        builder.Property(p => p.NetToFreelancer).HasPrecision(18, 2);
        builder.Property(p => p.PlatformRevenue).HasPrecision(18, 2);
        builder.Property(p => p.GatewayRef).HasMaxLength(120);

        builder.HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Proposal)
            .WithMany()
            .HasForeignKey(p => p.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Freelancer)
            .WithMany()
            .HasForeignKey(p => p.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PlatformFee)
            .WithMany()
            .HasForeignKey(p => p.PlatformFeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Um Payment por proposta aceita.
        builder.HasIndex(p => p.ProposalId).IsUnique();
        builder.HasIndex(p => p.ProjectId);
        builder.HasIndex(p => p.Status);
        // ClientId e FreelancerId NÃO precisam de índice single-column: são
        // a coluna líder dos compostos abaixo (Postgres usa o prefixo).
        // Dashboards (ganhos do mês, gasto na plataforma) — filtros compostos quentes.
        builder.HasIndex(p => new { p.ClientId, p.Status });
        builder.HasIndex(p => new { p.FreelancerId, p.Status, p.ReleasedAt });
    }
}
