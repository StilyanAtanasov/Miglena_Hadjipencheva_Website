using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MHAuthorWebsite.Core.Background_Services;

public class ScheduledNotificationIntegrityService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ScheduledNotificationIntegrityService> _logger;

    public ScheduledNotificationIntegrityService(IServiceProvider services, ILogger<ScheduledNotificationIntegrityService> logger)
    {
        _services = services;
        _logger = logger;
    }

    private TimeSpan DelayInterval => TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _services.CreateScope();
                IApplicationRepository repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                IScheduledNotificationIntegrityDataService dataService =
                    scope.ServiceProvider.GetRequiredService<IScheduledNotificationIntegrityDataService>();

                ICollection<ScheduledNotification> expiredNotifications = await dataService.GetAllExpiredPendingScheduledNotificationsAsync();
                foreach (ScheduledNotification notification in expiredNotifications) notification.NotificationStatus = ScheduledNotificationStatus.Expired;

                await repository.SaveChangesAsync();
                _logger.LogInformation($"Successfully changed {expiredNotifications.Count} notifications to expired status.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred: {ex.Message}");
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}