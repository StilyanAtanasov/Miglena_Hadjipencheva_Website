using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class AdminNotificationPreferenceConfiguration : IEntityTypeConfiguration<AdminNotificationPreference>
{
    public void Configure(EntityTypeBuilder<AdminNotificationPreference> builder)
    {
        builder
            .HasOne(p => p.User)
            .WithOne(u => u.AdminNotificationPreference)
            .HasForeignKey<AdminNotificationPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .Property(p => p.ReceiveNewOrderEmails)
            .HasDefaultValue(true);

        builder
            .Property(p => p.ReceiveContactRequestEmails)
            .HasDefaultValue(true);

        builder
            .Property(p => p.ReceiveServerErrorEmails)
            .HasDefaultValue(true);
    }
}
