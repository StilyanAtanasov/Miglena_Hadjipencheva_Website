using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ScheduledNotificationConfiguration : IEntityTypeConfiguration<ScheduledNotification>
{
    public void Configure(EntityTypeBuilder<ScheduledNotification> builder)
    {
        builder
            .Property(n => n.Subject)
            .HasDefaultValue("Ново Известие");

        builder
            .Property(n => n.ExpirationDate)
            .IsRequired()
            .HasDefaultValueSql("DATEADD(day, 30, GETDATE())");

        builder
            .HasQueryFilter(n => n.NotificationStatus != ScheduledNotificationStatus.Expired);
    }
}