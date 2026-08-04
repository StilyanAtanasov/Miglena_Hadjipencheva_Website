using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder
            .Property(u => u.IsBanned)
            .HasDefaultValue(false);

        builder
            .Property(u => u.IsMarketingSubscribed)
            .HasDefaultValue(false);

        builder
            .Property(u => u.HasAcceptedPrivacyPolicy)
            .HasDefaultValue(false);
    }
}
