using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.DataServices;

public class ScheduledNotificationIntegrityDataService : IScheduledNotificationIntegrityDataService
{
    private readonly IApplicationRepository _repository;

    public ScheduledNotificationIntegrityDataService(IApplicationRepository repository) => _repository = repository;

    public async Task<ICollection<ScheduledNotification>> GetAllExpiredPendingScheduledNotificationsAsync()
     => await _repository
         .Where<ScheduledNotification>(n =>
             n.NotificationStatus == ScheduledNotificationStatus.Pending
             && n.ExpirationDate < DateTime.Now)
         .IgnoreQueryFilters()
         .ToArrayAsync();
}