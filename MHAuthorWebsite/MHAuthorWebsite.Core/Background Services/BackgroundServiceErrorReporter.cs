using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Error;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MHAuthorWebsite.Core.Background_Services;

internal static class BackgroundServiceErrorReporter
{
    public static async Task ReportAsync(
        IServiceProvider services,
        Exception exception,
        string source,
        ILogger logger)
    {
        try
        {
            using IServiceScope scope = services.CreateScope();
            IErrorService errorService = scope.ServiceProvider.GetRequiredService<IErrorService>();

            await errorService.HandleErrorAsync(new HandleErrorDto
            {
                Exception = exception,
                Path = source,
                RequestId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N"),
                UserId = null,
                UserNameIdentifier = "BackgroundService",
                Method = "BACKGROUND_TASK"
            });
        }
        catch (Exception reportException)
        {
            logger.LogCritical(reportException,
                "CRITICAL: Failed to notify admins for background service error in {Source}.",
                source);
        }
    }
}
