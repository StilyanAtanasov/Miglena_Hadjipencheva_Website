using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Background_Services.Data_Services;

public interface IScheduledEmailNotificationSenderDataService
{
    Task<ICollection<ScheduledNotification>> GetPendingEmailNotificationsAsync(CancellationToken cancellationToken);
}