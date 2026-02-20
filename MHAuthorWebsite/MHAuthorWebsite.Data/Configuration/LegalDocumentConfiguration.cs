using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> builder)
    {
        builder
            .HasIndex(d => new { d.DocumentType, d.Version })
            .IsUnique();

        builder
            .HasOne(d => d.Admin)
            .WithMany()
            .HasForeignKey(d => d.AdminId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
