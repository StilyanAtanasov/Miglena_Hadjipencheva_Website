using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder
            .HasMany(p => p.Products)
            .WithMany(o => o.Orders)
            .UsingEntity<Dictionary<string, object>>(
                "OrdersProducts",
                j => j
                    .HasOne<Product>()
                    .WithMany()
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<Order>()
                    .WithMany()
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("OrderId", "ProductId");
                    j.ToTable("OrdersProducts");
                });

        builder
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}