namespace MHAuthorWebsite.Core.Dtos.Admin.UserManagement;

public class UserDetailsDto
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsBanned { get; set; }

    public bool HasAcceptedPrivacyPolicy { get; set; }

    public DateTime? PrivacyPolicyAcceptedOn { get; set; }

    public string? PrivacyPolicyVersion { get; set; }

    public bool IsMarketingSubscribed { get; set; }

    public DateTime? MarketingSubscribedOn { get; set; }

    public DateTime? MarketingUnsubscribedOn { get; set; }

    public bool HasMarketingUnsubscribeToken { get; set; }

    public DateTime? MarketingUnsubscribeTokenCreatedOn { get; set; }

    public bool? ReceiveNewOrderEmails { get; set; }

    public bool? ReceiveContactRequestEmails { get; set; }

    public bool? ReceiveServerErrorEmails { get; set; }

    public DateTime? AdminNotificationPreferencesUpdatedOn { get; set; }

    public DateTime DateJoined { get; set; }

    public DateTime LastActive { get; set; }
}
