using Decidi.Domain.Common;
using Decidi.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Decidi.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<FreelancerProfile> FreelancerProfiles => Set<FreelancerProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PlatformFee> PlatformFees => Set<PlatformFee>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Sprint 7.2 — Enforce UTC global em todos os DateTime/DateTime?.
        // Lê: re-rotula Kind=Utc (Postgres timestamptz já é UTC).
        // Grava: exige UTC explícito — Kind=Unspecified lança (evita bug latente
        // "10:00 sem Z" virar "10:00 UTC" silenciosamente).
        var utc = new ValueConverter<DateTime, DateTime>(
            v => ToUtcForStorage(v),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        var utcNullable = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? ToUtcForStorage(v.Value) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(utc);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(utcNullable);
            }
        }
    }

    /// <summary>
    /// Garante que um DateTime indo para o banco esteja em UTC. Kind=Unspecified é rejeitado
    /// para evitar interpretações implícitas que dependem do TZ do servidor.
    /// </summary>
    private static DateTime ToUtcForStorage(DateTime v) =>
        v.Kind == DateTimeKind.Utc ? v
            : v.Kind == DateTimeKind.Local ? v.ToUniversalTime()
            : throw new InvalidOperationException(
                "DateTime com Kind=Unspecified não pode ser persistido. Use DateTime.UtcNow ou DateTime.SpecifyKind(Utc) explicitamente.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        // Rotaciona RowVersion das entidades com concurrency token toda vez que sofrem update.
        foreach (var entry in ChangeTracker.Entries<Project>())
            if (entry.State == EntityState.Modified) entry.Entity.RowVersion = Guid.NewGuid();
        foreach (var entry in ChangeTracker.Entries<Proposal>())
            if (entry.State == EntityState.Modified) entry.Entity.RowVersion = Guid.NewGuid();

        return base.SaveChangesAsync(cancellationToken);
    }
}
