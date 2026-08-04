using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Core.NotificationTemplates;
using RazorLight;
using System.Reflection;
using System.Text.Json;

namespace MHAuthorWebsite.Infrastructure.Rendering;

public class RazorLightRenderingService : INotificationRenderingService
{
    private readonly IRazorLightEngine _engine;

    public RazorLightRenderingService(IRazorLightEngine engine) => _engine = engine;

    public async Task<string> RenderNotificationAsync(ScheduledNotification notification)
    {
        Type enumType = typeof(ScheduledNotificationTemplate);
        MemberInfo[] memberInfo = enumType.GetMember(notification.NotificationTemplate.ToString());
        ScheduledNotificationDataAttribute? attribute = memberInfo[0].GetCustomAttribute<ScheduledNotificationDataAttribute>();

        if (attribute == null)
            throw new InvalidOperationException("Template metadata missing on Enum.");

        object? payloadModel = JsonSerializer.Deserialize(notification.Payload!, attribute.PayloadType);

        string html = await _engine.CompileRenderAsync(attribute.TemplateFile, payloadModel);

        return html;
    }
}
