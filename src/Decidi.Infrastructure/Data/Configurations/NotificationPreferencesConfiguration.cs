using Decidi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Decidi.Infrastructure.Data.Configurations;

public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
{
    public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
    {
        builder.Property(n => n.UserId).IsRequired();
        builder.HasIndex(n => n.UserId).IsUnique();
        builder.HasOne(n => n.User)
            .WithOne()
            .HasForeignKey<NotificationPreferences>(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
