using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MHAuthorWebsite.Data.Models;

public class Cart
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    [Required]
    public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
}