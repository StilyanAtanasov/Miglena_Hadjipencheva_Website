using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Background_Services.Data_Services;

public interface IScheduledNotificationIntegrityDataService
{
    Task<ICollection<ScheduledNotification>> GetAllExpiredPendingScheduledNotificationsAsync();
}