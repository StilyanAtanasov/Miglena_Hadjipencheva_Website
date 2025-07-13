using MHAuthorWebsite.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static MHAuthorWebsite.GCommon.EntityConstraints.Product;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .HasMany(p => p.Likes)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ProductsLikes",
                j => j
                    .HasOne<IdentityUser>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict),
                j => j
                    .HasOne<Product>()
                    .WithMany()
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Restrict),
                j =>
                {
                    j.HasKey("UserId", "ProductId");
                    j.ToTable("ProductsLikes");
                });

        builder
            .HasOne(p => p.ProductType)
            .WithMany(pt => pt.Products)
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(p => p.IsPublic)
            .HasDefaultValue(IsPublicDefaultValue);

        builder
            .Property(p => p.IsDeleted)
            .HasDefaultValue(IsDeletedDefaultValue);

        builder
            .HasQueryFilter(p => !p.IsDeleted);
    }
}