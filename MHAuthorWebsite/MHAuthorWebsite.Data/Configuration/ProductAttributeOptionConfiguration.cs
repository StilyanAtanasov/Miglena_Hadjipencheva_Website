using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class ProductAttributeOptionConfiguration : IEntityTypeConfiguration<ProductAttributeOption>
{
    public void Configure(EntityTypeBuilder<ProductAttributeOption> builder)
    {
        builder
            .HasOne(po => po.AttributeDefinition)
            .WithMany(def => def.ProductAttributeOptions)
            .HasForeignKey(po => po.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}