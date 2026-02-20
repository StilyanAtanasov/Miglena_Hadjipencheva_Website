using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Contracts;

public interface IAdminNotificationPreferencesService
{
    Task<AdminNotificationPreferencesDto> GetAdminPreferencesAsync(string userId);

    Task<ServiceResult<AdminNotificationPreferencesDto>> UpdateAdminPreferenceAsync(
        string userId,
        AdminNotificationType notificationType,
        bool isEnabled);

    Task<ICollection<string>> GetAdminEmailsForNotificationAsync(AdminNotificationType notificationType);
}
