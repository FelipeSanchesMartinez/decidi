using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(5000).IsRequired();
        builder.Property(p => p.BudgetMin).HasPrecision(18, 2);
        builder.Property(p => p.BudgetMax).HasPrecision(18, 2);

        builder.HasOne(p => p.Client)
            .WithMany(u => u.ClientProjects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AcceptedFreelancer)
            .WithMany()
            .HasForeignKey(p => p.AcceptedFreelancerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.RequiredSkills)
            .WithMany(s => s.Projects);

        // Status single-column NÃO precisa: é prefixo do composto (Status, CategoryId, CreatedAt).
        builder.HasIndex(p => p.CreatedAt);
        // Busca pública: filtra por Status + Categoria, ordena por mais recentes.
        builder.HasIndex(p => new { p.Status, p.CategoryId, p.CreatedAt });
        // Meus Projetos (cliente) filtrando por status.
        builder.HasIndex(p => new { p.ClientId, p.Status });
        // Dashboard freelancer: contratos ativos.
        builder.HasIndex(p => new { p.AcceptedFreelancerId, p.Status });

        builder.Property(p => p.RowVersion).IsConcurrencyToken();
    }
}
