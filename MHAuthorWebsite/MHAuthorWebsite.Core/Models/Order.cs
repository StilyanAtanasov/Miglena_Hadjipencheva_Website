using MHAuthorWebsite.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Core.Models;

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    public Shipment Shipment { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; }

    public ICollection<OrderProduct> OrderedProducts { get; set; } = new HashSet<OrderProduct>();
}