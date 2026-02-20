using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage;

[SecurityHeaders(CspFeature.Notifications)]
public class EmailPreferencesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminNotificationPreferencesService _adminNotificationPreferencesService;

    public EmailPreferencesModel(
        UserManager<ApplicationUser> userManager,
        IAdminNotificationPreferencesService adminNotificationPreferencesService)
    {
        _userManager = userManager;
        _adminNotificationPreferencesService = adminNotificationPreferencesService;
    }

    [BindProperty]
    public bool IsMarketingSubscribed { get; set; }

    public bool IsAdmin { get; set; }

    public bool ReceiveNewOrderEmails { get; set; }

    public bool ReceiveContactRequestEmails { get; set; }

    public bool ReceiveServerErrorEmails { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user is null)
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        IsMarketingSubscribed = user.IsMarketingSubscribed;
        IsAdmin = await _userManager.IsInRoleAsync(user, MHAuthorWebsite.GCommon.ApplicationRules.Roles.AdminRoleName);

        if (IsAdmin)
        {
            AdminNotificationPreferencesDto adminPreferences =
                await _adminNotificationPreferencesService.GetAdminPreferencesAsync(user.Id);
            ReceiveNewOrderEmails = adminPreferences.ReceiveNewOrderEmails;
            ReceiveContactRequestEmails = adminPreferences.ReceiveContactRequestEmails;
            ReceiveServerErrorEmails = adminPreferences.ReceiveServerErrorEmails;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostToggleMarketingAsync([FromForm] bool isEnabled)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user is null)
            return JsonError("Профилът не е намерен.", StatusCodes.Status404NotFound);

        user.IsMarketingSubscribed = isEnabled;

        if (isEnabled)
        {
            user.MarketingSubscribedOn = DateTime.UtcNow;
            user.MarketingUnsubscribedOn = null;
            user.MarketingUnsubscribeToken = GenerateOneTimeToken();
            user.MarketingUnsubscribeTokenCreatedOn = DateTime.UtcNow;
        }
        else
        {
            user.MarketingUnsubscribedOn = DateTime.UtcNow;
            user.MarketingUnsubscribeToken = null;
            user.MarketingUnsubscribeTokenCreatedOn = null;
        }

        IdentityResult result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return JsonError("Неуспешно запазване на предпочитанията.", StatusCodes.Status400BadRequest);

        string message = isEnabled
            ? "Абонаментът за маркетинг имейли е активиран."
            : "Абонаментът за маркетинг имейли е спрян.";

        return new JsonResult(new
        {
            success = true,
            isEnabled,
            message = message
        });
    }

    public async Task<IActionResult> OnPostToggleAdminNotificationAsync([FromForm] string notificationType, [FromForm] bool isEnabled)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user is null)
            return JsonError("Профилът не е намерен.", StatusCodes.Status404NotFound);

        if (!await _userManager.IsInRoleAsync(user, MHAuthorWebsite.GCommon.ApplicationRules.Roles.AdminRoleName))
            return JsonError("Нямате достъп до администраторските известия.", StatusCodes.Status403Forbidden);

        bool parsed = Enum.TryParse(notificationType, ignoreCase: true, out AdminNotificationType parsedNotificationType);
        if (!parsed)
            return JsonError("Невалиден тип известие.", StatusCodes.Status400BadRequest);

        Core.Common.Utils.ServiceResult<AdminNotificationPreferencesDto> updateResult =
            await _adminNotificationPreferencesService.UpdateAdminPreferenceAsync(user.Id, parsedNotificationType, isEnabled);
        if (!updateResult.Success)
            return JsonError("Неуспешно запазване на администраторските известия.", StatusCodes.Status400BadRequest);

        string message = isEnabled
            ? "Известието е активирано."
            : "Известието е спряно.";

        return new JsonResult(new
        {
            success = true,
            isEnabled,
            message = message
        });
    }

    private static JsonResult JsonError(string message, int statusCode)
        => new(new { success = false, message = message }) { StatusCode = statusCode };

    private static string GenerateOneTimeToken()
        => $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
}
