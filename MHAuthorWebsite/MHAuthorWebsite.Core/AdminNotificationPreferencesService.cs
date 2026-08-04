using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core;

public class AdminNotificationPreferencesService : IAdminNotificationPreferencesService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminNotificationPreferencesService> _logger;

    public AdminNotificationPreferencesService(
        IApplicationRepository repository,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminNotificationPreferencesService> logger)
    {
        _repository = repository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AdminNotificationPreferencesDto> GetAdminPreferencesAsync(string userId)
    {
        AdminNotificationPreference preference = await GetOrCreatePreferenceForAdminAsync(userId);
        return Map(preference);
    }

    public async Task<ServiceResult<AdminNotificationPreferencesDto>> UpdateAdminPreferenceAsync(
        string userId,
        AdminNotificationType notificationType,
        bool isEnabled)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null || !await _userManager.IsInRoleAsync(user, AdminRoleName))
            return ServiceResult<AdminNotificationPreferencesDto>.Forbidden();

        AdminNotificationPreference preference = await GetOrCreatePreferenceForAdminAsync(userId);

        switch (notificationType)
        {
            case AdminNotificationType.NewOrder:
                preference.ReceiveNewOrderEmails = isEnabled;
                break;
            case AdminNotificationType.ContactRequest:
                preference.ReceiveContactRequestEmails = isEnabled;
                break;
            case AdminNotificationType.ServerError:
                preference.ReceiveServerErrorEmails = isEnabled;
                break;
            default:
                return ServiceResult<AdminNotificationPreferencesDto>.BadRequest(
                    new() { [nameof(notificationType)] = "Невалиден тип на известие." });
        }

        preference.UpdatedOn = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Admin notification preference updated. UserId: {UserId}, Type: {Type}, IsEnabled: {IsEnabled}",
            userId,
            notificationType,
            isEnabled);

        return ServiceResult<AdminNotificationPreferencesDto>.Ok(Map(preference));
    }

    public async Task<ICollection<string>> GetAdminEmailsForNotificationAsync(AdminNotificationType notificationType)
    {
        ICollection<ApplicationUser> adminsInRole = await _userManager.GetUsersInRoleAsync(AdminRoleName);
        ApplicationUser[] eligibleAdmins = adminsInRole
            .Where(a => a is { Email: not null, IsDeleted: false, IsBanned: false, EmailConfirmed: true })
            .ToArray();

        if (eligibleAdmins.Length == 0)
            return Array.Empty<string>();

        string[] adminIds = eligibleAdmins.Select(a => a.Id).ToArray();
        AdminNotificationPreference[] preferences = await _repository
            .WhereReadonly<AdminNotificationPreference>(p => adminIds.Contains(p.UserId))
            .ToArrayAsync();

        Dictionary<string, AdminNotificationPreference> preferencesByUserId = preferences
            .ToDictionary(p => p.UserId, StringComparer.Ordinal);

        List<string> emails = new();
        foreach (ApplicationUser admin in eligibleAdmins)
        {
            bool isEnabled = true;
            if (preferencesByUserId.TryGetValue(admin.Id, out AdminNotificationPreference? preference))
                isEnabled = IsEnabledForType(preference, notificationType);

            if (isEnabled)
                emails.Add(admin.Email!);
        }

        return emails
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<AdminNotificationPreference> GetOrCreatePreferenceForAdminAsync(string userId)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null || !await _userManager.IsInRoleAsync(user, AdminRoleName))
            throw new InvalidOperationException("Preferences can be created only for existing admins.");

        AdminNotificationPreference? preference = await _repository
            .Where<AdminNotificationPreference>(p => p.UserId == userId)
            .FirstOrDefaultAsync();

        if (preference is not null)
            return preference;

        preference = new AdminNotificationPreference
        {
            UserId = userId,
            ReceiveNewOrderEmails = true,
            ReceiveContactRequestEmails = true,
            ReceiveServerErrorEmails = true,
            UpdatedOn = DateTime.UtcNow
        };

        await _repository.AddAsync(preference);
        await _repository.SaveChangesAsync();
        return preference;
    }

    private static bool IsEnabledForType(AdminNotificationPreference preference, AdminNotificationType notificationType)
        => notificationType switch
        {
            AdminNotificationType.NewOrder => preference.ReceiveNewOrderEmails,
            AdminNotificationType.ContactRequest => preference.ReceiveContactRequestEmails,
            AdminNotificationType.ServerError => preference.ReceiveServerErrorEmails,
            _ => true
        };

    private static AdminNotificationPreferencesDto Map(AdminNotificationPreference preference)
        => new()
        {
            ReceiveNewOrderEmails = preference.ReceiveNewOrderEmails,
            ReceiveContactRequestEmails = preference.ReceiveContactRequestEmails,
            ReceiveServerErrorEmails = preference.ReceiveServerErrorEmails
        };
}
