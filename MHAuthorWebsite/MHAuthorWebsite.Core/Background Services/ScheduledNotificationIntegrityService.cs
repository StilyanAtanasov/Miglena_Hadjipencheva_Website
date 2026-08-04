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
        await Task.Delay(TimeSpan.FromSeconds(new Random().Next(35, 45)), cancellationToken);

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
                _logger.LogError(ex, "Error occurred in {ServiceName}.", nameof(ScheduledNotificationIntegrityService));
                await BackgroundServiceErrorReporter.ReportAsync(
                    _services,
                    ex,
                    nameof(ScheduledNotificationIntegrityService),
                    _logger);
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}
