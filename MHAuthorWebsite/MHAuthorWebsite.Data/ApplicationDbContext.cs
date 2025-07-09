using MHAuthorWebsite.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MHAuthorWebsite.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comments { get; set; } = null!;

    public DbSet<Image> Images { get; set; } = null!;

    public DbSet<Order> Orders { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<ProductAttribute> ProductAttributes { get; set; } = null!;

    public DbSet<ProductAttributeDefinition> ProductAttributeDefinitions { get; set; } = null!;

    public DbSet<ProductAttributeOption> ProductAttributeOptions { get; set; } = null!;

    public DbSet<ProductType> ProductTypes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}