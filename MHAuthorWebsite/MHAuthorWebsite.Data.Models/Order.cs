using MHAuthorWebsite.Data.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public Shipment Shipment { get; set; } = null!;

    [Required]
    [Comment("Order timestamp")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; }

    public ICollection<OrderProduct> OrderedProducts { get; set; } = new HashSet<OrderProduct>();
}