using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class UserLegalAgreementConfiguration : IEntityTypeConfiguration<UserLegalAgreement>
{
    public void Configure(EntityTypeBuilder<UserLegalAgreement> builder)
    {
        builder
            .HasOne(a => a.User)
            .WithMany(u => u.LegalAgreements)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(a => a.Document)
            .WithMany(d => d.UserAgreements)
            .HasForeignKey(a => a.DocumentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(a => new { a.UserId, a.DocumentType, a.DocumentVersion })
            .IsUnique();
    }
}
