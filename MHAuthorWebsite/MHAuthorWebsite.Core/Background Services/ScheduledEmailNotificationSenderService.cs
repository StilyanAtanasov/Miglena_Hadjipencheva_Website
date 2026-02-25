using MHAuthorWebsite.Core.Background_Services.Data_Services;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MHAuthorWebsite.Core.Background_Services;

public class ScheduledEmailNotificationSenderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ScheduledEmailNotificationSenderService> _logger;

    public ScheduledEmailNotificationSenderService(IServiceProvider services, ILogger<ScheduledEmailNotificationSenderService> logger)
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

                _logger.LogInformation($"Successfully send {sentCount} notifications!");
                _logger.LogInformation($"{failedCount} notifications failed to send (User not found)!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {ServiceName}.", nameof(ScheduledEmailNotificationSenderService));
                await BackgroundServiceErrorReporter.ReportAsync(
                    _services,
                    ex,
                    nameof(ScheduledEmailNotificationSenderService),
                    _logger);
            }

            await Task.Delay(DelayInterval, cancellationToken);
        }
    }
}
