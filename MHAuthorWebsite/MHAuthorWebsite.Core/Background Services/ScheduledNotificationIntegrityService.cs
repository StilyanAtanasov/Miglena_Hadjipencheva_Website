using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MHAuthorWebsite.Core.Background_Services;

public class ScheduledNotificationIntegrityService : BackgroundService
{
    private readonly IServiceProvider _services;

    public ScheduledNotificationIntegrityService(IServiceProvider services) => _services = services;

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
                Console.WriteLine($"Successfully changed {expiredNotifications.Count} notifications to expired status.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}