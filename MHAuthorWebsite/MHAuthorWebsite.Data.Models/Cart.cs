using System.ComponentModel.DataAnnotations;

namespace MHAuthorWebsite.Data.Models;

public class Cart
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
}