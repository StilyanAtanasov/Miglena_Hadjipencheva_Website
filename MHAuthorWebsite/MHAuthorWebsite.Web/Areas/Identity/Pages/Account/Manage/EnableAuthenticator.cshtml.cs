// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage;

[SecurityHeaders(CspFeature.QrCodes)]
public class EnableAuthenticatorModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EnableAuthenticatorModel> _logger;
    private readonly UrlEncoder _urlEncoder;
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    public EnableAuthenticatorModel(
        UserManager<ApplicationUser> userManager,
        ILogger<EnableAuthenticatorModel> logger,
        UrlEncoder urlEncoder,
        IEmailService emailService,
        IEmailUserProvider emailUserProvider)
    {
        _userManager = userManager;
        _logger = logger;
        _urlEncoder = urlEncoder;
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string SharedKey { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string AuthenticatorUri { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string[] RecoveryCodes { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Кодът за потвърждение е задължителен.")]
        [StringLength(7, ErrorMessage = "Кодът за потвърждение трябва да бъде между {2} и {1} символа.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Код за потвърждение")]
        public string Code { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadSharedKeyAndQrCodeUriAsync(user);

        bool is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        ViewData["IsTwoFactorEnabled"] = is2FaEnabled;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        // Strip spaces and hyphens
        var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2FaTokenValid)
        {
            ModelState.AddModelError("Input.Code", "Кодът за потвърждение е невалиден.");
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var userId = await _userManager.GetUserIdAsync(user);
        _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

        StatusMessage = "Вашето приложение за автентификация беше потвърдено.";

        if (user.Email is not null)
        {
            string tfaEnabledBody = $@"
                <!DOCTYPE html>
                <html lang=""bg"">
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                        .status-badge {{ display: inline-block; background-color: #d4edda; color: #155724; padding: 5px 15px; border-radius: 20px; font-weight: bold; margin: 10px 0; }}
                    </style>
                </head>
                <body style=""margin: 0; padding: 0; background-color: #fcfcfc; font-family: 'Segoe UI', Arial, sans-serif;"">
                    <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""100%"" style=""background-color: #fcfcfc;"">
                        <tr>
                            <td align=""center"" style=""padding: 20px 0;"">
                                <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" width=""600"" style=""background-color: #ffffff; border: 1px solid #f0f0f0; border-radius: 8px;"">
                                    <tr>
                                        <td style=""padding: 20px; background-color: #3a053a; border-radius: 8px 8px 0 0; text-align: center;"">
                                            <span style=""color: #fcfcfc; font-size: 20px; font-weight: bold; letter-spacing: 1px;"">
                                                Двуфакторната автентикация е активна
                                            </span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 40px 30px;"">
                                            <div class=""content-text"">
                                                <p>Здравейте, {user.Name?.Split(" ")[0]},</p>
                                                <p>Двуфакторната автентикация (2FA) за Вашия акаунт беше активирана успешно.</p>
                                                <div class=""status-badge"">Статус: Активен</div>
                                                <p>Сега Вашият профил е по-защитен. При всяко влизане ще трябва да предоставяте код, генериран от Вашето приложение за автентикация.</p>
                                                <p><strong>Важно:</strong> Уверете се, че сте запазили Вашите кодове за възстановяване на сигурно място.</p>
                                            </div>

                                            <p style=""font-size: 14px; color: #999; margin-top: 30px;"">
                                                С уважение,<br>
                                                Екипът на {WebsiteName}
                                            </p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 20px; background-color: #fbf3fb; text-align: center; font-size: 12px; color: #616161; border-radius: 0 0 8px 8px;"">
                                            <p style=""margin: 0;"">Ако не сте активирали тази опция, моля свържете се с нас веднага.</p>
                                            <p style=""margin: 5px 0 0 0;"">&copy; {DateTime.UtcNow.Year} {WebsiteName}</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await _emailService.SendEmailAsync(
                _emailUserProvider.GetNotificationsUser(),
                user.Email,
                "Двуфакторната автентикация бе успешно активирана!",
                tfaEnabledBody,
                true);
        }

        if (await _userManager.CountRecoveryCodesAsync(user) == 0)
        {
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            if (recoveryCodes != null) RecoveryCodes = recoveryCodes.ToArray();
            return RedirectToPage("./ShowRecoveryCodes");
        }

        return RedirectToPage("./TwoFactorAuthentication");
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
    {
        // Load the authenticator key & QR code URI to display on the form
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        SharedKey = FormatKey(unformattedKey);

        string email = await _userManager.GetEmailAsync(user);
        AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey);
    }

    private string FormatKey(string unformattedKey)
    {
        StringBuilder result = new();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            _urlEncoder.Encode(WebsiteName),
            _urlEncoder.Encode(email),
            unformattedKey);
    }
}