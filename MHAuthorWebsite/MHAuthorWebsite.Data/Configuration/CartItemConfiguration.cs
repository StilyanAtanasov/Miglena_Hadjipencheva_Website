using MHAuthorWebsite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static MHAuthorWebsite.GCommon.EntityConstraints.CartItem;

namespace MHAuthorWebsite.Data.Configuration;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder
            .Property(ci => ci.IsSelected)
            .HasDefaultValue(IsSelectedDefaultValue);

        builder
            .HasQueryFilter(ci => !ci.Product.IsDeleted);
    }
}