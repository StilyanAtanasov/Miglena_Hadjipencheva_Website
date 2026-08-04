namespace MHAuthorWebsite.Core.NotificationTemplates;

[AttributeUsage(AttributeTargets.Field)]
public class ScheduledNotificationDataAttribute : Attribute
{
    public ScheduledNotificationDataAttribute(string templateFile, Type payloadType)
    {
        TemplateFile = templateFile;
        PayloadType = payloadType;
    }

    public string TemplateFile { get; }

    public Type PayloadType { get; }
}