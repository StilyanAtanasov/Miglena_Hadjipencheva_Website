using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.EntityConstraints.ApplicationUser;

namespace MHAuthorWebsite.Core.Models;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [MaxLength(NameMaxLength)]
    public string? Name { get; set; }

    public string? PendingEmail { get; set; }

    public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; }

    public bool IsBanned { get; set; }

    public bool IsMarketingSubscribed { get; set; }

    public DateTime? MarketingSubscribedOn { get; set; }

    public DateTime? MarketingUnsubscribedOn { get; set; }

    [MaxLength(MarketingUnsubscribeTokenMaxLength)]
    public string? MarketingUnsubscribeToken { get; set; }

    public DateTime? MarketingUnsubscribeTokenCreatedOn { get; set; }

    public bool HasAcceptedPrivacyPolicy { get; set; }

    public DateTime? PrivacyPolicyAcceptedOn { get; set; }

    [MaxLength(PrivacyPolicyVersionMaxLength)]
    public string? PrivacyPolicyVersion { get; set; }

    public ICollection<ProductComment> ProductComments { get; set; } = new HashSet<ProductComment>();

    public ICollection<ProductCommentReaction> ProductCommentsReactions { get; set; } = new HashSet<ProductCommentReaction>();

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

    public ICollection<Product> LikedProducts { get; set; } = new HashSet<Product>();

    public ICollection<Cart> Carts { get; set; } = new HashSet<Cart>();

    public ICollection<AnnouncementEmailDelivery> AnnouncementEmailDeliveries { get; set; } = new HashSet<AnnouncementEmailDelivery>();

    public AdminNotificationPreference? AdminNotificationPreference { get; set; }

    public ICollection<UserLegalAgreement> LegalAgreements { get; set; } = new HashSet<UserLegalAgreement>();
}
