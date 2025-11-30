using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder
            .Property(pa => pa.DisplayPosition)
            .HasDefaultValue(ProductAttributeDisplayPosition.AdditionalInfoTable);

        builder
            .HasOne(a => a.Product)
            .WithMany(p => p.Attributes)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasQueryFilter(pa => !pa.Product.IsDeleted && pa.Product.IsPublic);
    }
}