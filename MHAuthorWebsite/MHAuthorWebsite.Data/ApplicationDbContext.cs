using MHAuthorWebsite.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MHAuthorWebsite.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductComment> ProductComments { get; set; } = null!;

    public DbSet<ProductCommentReaction> ProductCommentsReactions { get; set; } = null!;

    public DbSet<ProductCommentImage> ProductCommentsImages { get; set; } = null!;

    public DbSet<ProductImage> ProductsImages { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;

    public DbSet<OrderProduct> OrdersProducts { get; set; } = null!;

    public DbSet<Shipment> Shipments { get; set; } = null!;

    public DbSet<ShipmentEvent> ShipmentEvents { get; set; } = null!;

    public DbSet<ShipmentService> ShipmentServices { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<ProductAttribute> ProductAttributes { get; set; } = null!;

    public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions { get; set; } = null!;

    public DbSet<ProductAttributeOption> ProductAttributeOptions { get; set; } = null!;

    public DbSet<ProductType> ProductTypes { get; set; } = null!;

    public DbSet<Cart> Carts { get; set; } = null!;

    public DbSet<CartItem> CartItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}