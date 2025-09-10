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

    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

    public ICollection<Product> LikedProducts { get; set; } = new HashSet<Product>();
}