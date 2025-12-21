using MHAuthorWebsite.Core.NotificationTemplates.PayloadModels.Contracts;

namespace MHAuthorWebsite.Core.NotificationTemplates;

[AttributeUsage(AttributeTargets.Field)]
public class ScheduledNotificationDataAttribute : Attribute
{
    public ScheduledNotificationDataAttribute(string templateFile, IScheduledNotificationPayload payloadType)
    {
        TemplateFile = templateFile;
        PayloadType = payloadType;
    }

    public string TemplateFile { get; }

    public IScheduledNotificationPayload PayloadType { get; }
}