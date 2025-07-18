using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static MHAuthorWebsite.GCommon.EntityConstraints.Order;

namespace MHAuthorWebsite.Data.Models;

public class Order
{
    [Key]
    [Comment("Primary key")]
    public Guid Id { get; set; }

    [Required]
    [Comment("Foreign key to User")]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public IdentityUser User { get; set; } = null!;

    [Required]
    [Comment("Order timestamp")]
    public DateTime Date { get; set; }

    [Required]
    [Column(TypeName = PriceSqlType)]
    [Comment("Total order price")]
    public decimal Price { get; set; }

    public ICollection<Product> Products { get; set; } = new HashSet<Product>();

    [Comment("Soft delete flag")]
    public bool IsDeleted { get; set; }
}