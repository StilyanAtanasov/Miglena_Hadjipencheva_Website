using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ApplicationUser;

namespace MHAuthorWebsite.Data.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [MaxLength(NameMaxLength)]
    public string? Name { get; set; }

    public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public ICollection<ProductComment> ProductComments { get; set; } = new HashSet<ProductComment>();

    public ICollection<ProductCommentReaction> ProductCommentsReactions { get; set; } = new HashSet<ProductCommentReaction>();

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

    public ICollection<Product> LikedProducts { get; set; } = new HashSet<Product>();

    public ICollection<Cart> Carts { get; set; } = new HashSet<Cart>();
}