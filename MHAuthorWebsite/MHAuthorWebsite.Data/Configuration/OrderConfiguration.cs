using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MHAuthorWebsite.Data.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(o => o.Status)
            .HasDefaultValue(OrderStatus.InReview);
    }
}