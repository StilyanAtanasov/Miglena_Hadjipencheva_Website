using MHAuthorWebsite.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class OrderProductConfiguration : IEntityTypeConfiguration<OrderProduct>
{
    public void Configure(EntityTypeBuilder<OrderProduct> builder)
    {
        builder
            .HasKey(op => new { op.ProductId, op.OrderId });

        builder
            .HasQueryFilter(op => !op.Product.IsDeleted);
    }
}