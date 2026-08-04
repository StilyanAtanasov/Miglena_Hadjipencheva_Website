using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class AnnouncementEmailDeliveryConfiguration : IEntityTypeConfiguration<AnnouncementEmailDelivery>
{
    public void Configure(EntityTypeBuilder<AnnouncementEmailDelivery> builder)
    {
        builder
            .HasOne(d => d.Announcement)
            .WithMany(a => a.EmailDeliveries)
            .HasForeignKey(d => d.AnnouncementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(d => d.User)
            .WithMany(u => u.AnnouncementEmailDeliveries)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(d => new { d.AnnouncementId, d.Email })
            .IsUnique();
    }
}
