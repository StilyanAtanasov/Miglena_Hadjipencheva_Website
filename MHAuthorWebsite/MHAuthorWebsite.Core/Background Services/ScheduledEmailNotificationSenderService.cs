using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MHAuthorWebsite.Core.Background_Services;

public class ScheduledEmailNotificationSenderService : BackgroundService
{
    private readonly IServiceProvider _services;

    public ScheduledEmailNotificationSenderService(IServiceProvider services) => _services = services;

    private TimeSpan DelayInterval => TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = _services.CreateScope();
                IApplicationRepository repository = scope.ServiceProvider.GetRequiredService<IApplicationRepository>();
                IScheduledEmailNotificationSenderDataService dataService =
                    scope.ServiceProvider.GetRequiredService<IScheduledEmailNotificationSenderDataService>();
                IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                IEmailUserProvider userProvider = scope.ServiceProvider.GetRequiredService<IEmailUserProvider>();
                INotificationRenderingService notificationRenderingService = scope.ServiceProvider.GetRequiredService<INotificationRenderingService>();

                ICollection<ScheduledNotification> scheduledEmails = await dataService.GetPendingEmailNotificationsAsync(cancellationToken);

                uint sentCount = 0;
                uint failedCount = 0;
                foreach (ScheduledNotification n in scheduledEmails)
                {
                    if (n.Recipient.Email == null)
                    {
                        n.NotificationStatus = ScheduledNotificationStatus.Failed;
                        failedCount++;
                        continue;
                    }

                    await emailService.SendEmailAsync(
                        userProvider.GetNotificationsUser(),
                        n.TargetDeliveryDetails ?? n.Recipient.Email,
                        n.Subject,
                       await notificationRenderingService.RenderNotificationAsync(n),
                        true);

                    n.NotificationStatus = ScheduledNotificationStatus.Sent;
                    n.SentAt = DateTime.UtcNow;
                    sentCount++;
                }

                await repository.SaveChangesAsync();

                Console.WriteLine($"Successfully send {sentCount} notifications!");
                Console.WriteLine($"{failedCount} notifications failed to send!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}