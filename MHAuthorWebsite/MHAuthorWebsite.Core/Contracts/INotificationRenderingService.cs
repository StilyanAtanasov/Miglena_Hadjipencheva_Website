using MHAuthorWebsite.Core.Models;

namespace MHAuthorWebsite.Core.Contracts;

public interface INotificationRenderingService
{
    Task<string> RenderNotificationAsync(ScheduledNotification notification);
}