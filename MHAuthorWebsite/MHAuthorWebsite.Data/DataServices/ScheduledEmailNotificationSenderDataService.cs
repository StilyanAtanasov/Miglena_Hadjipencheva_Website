using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class ScheduledEmailNotificationSenderDataService : IScheduledEmailNotificationSenderDataService
{
    private readonly IApplicationRepository _repository;

    public ScheduledEmailNotificationSenderDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<ICollection<ScheduledNotification>> GetPendingEmailNotificationsAsync(CancellationToken cancellationToken)
     => await _repository
         .Where<ScheduledNotification>(n =>
             n.NotificationStatus == ScheduledNotificationStatus.Pending
             && n.NotificationType == ScheduledNotificationType.Email)
         .Include(n => n.Recipient)
         .ToArrayAsync(cancellationToken);
}