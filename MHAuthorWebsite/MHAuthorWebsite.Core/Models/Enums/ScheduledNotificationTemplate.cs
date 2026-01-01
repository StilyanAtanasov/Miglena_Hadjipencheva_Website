using MHAuthorWebsite.Core.NotificationTemplates;
using MHAuthorWebsite.Core.NotificationTemplates.PayloadModels;

namespace MHAuthorWebsite.Core.Models.Enums;

public enum ScheduledNotificationTemplate
{
    [ScheduledNotificationData("OrderStatusUpdateNotificationTemplate.cshtml", typeof(OrderStatusUpdatePayloadModel))]
    OrderStatusUpdate = 0
}