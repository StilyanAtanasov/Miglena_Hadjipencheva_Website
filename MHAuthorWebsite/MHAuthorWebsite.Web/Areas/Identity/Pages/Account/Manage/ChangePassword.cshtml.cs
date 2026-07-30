// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage;

public class ChangePasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ChangePasswordModel> _logger;
    private readonly IEmailService _emailService;
    private readonly IEmailUserProvider _emailUserProvider;

    public ChangePasswordModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ChangePasswordModel> logger,
        IEmailService emailService,
        IEmailUserProvider emailUserProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _emailService = emailService;
        _emailUserProvider = emailUserProvider;
    }

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
    [TempData]
    public string StatusMessage { get; set; }

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
        [Required(ErrorMessage = "Текущата парола е задължителна.")]
        [DataType(DataType.Password)]
        [Display(Name = "Текуща парола")]
        public string OldPassword { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Новата парола е задължителна.")]
        [StringLength(100, ErrorMessage = "Паролата трябва да е поне {2} и максимум {1} символа!", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Потвърждението на новата парола е задължително.")]
        [DataType(DataType.Password)]
        [Display(Name = "Потвърди новата парола")]
        [Compare("NewPassword", ErrorMessage = "Двете пароли не съвпадат!")]
        public string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        ApplicationUser user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await _userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToPage("./SetPassword");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        ApplicationUser user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IdentityResult changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (IdentityError error in changePasswordResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User changed their password successfully.");

        if (string.IsNullOrEmpty(user.Email))
        {
            StatusMessage = "Вашата парола беше променена успешно.";
            return RedirectToPage();
        }

        string passwordChangedBody = $@"
            <!DOCTYPE html>
            <html lang=""bg"">
            <head>
                <meta charset=""UTF-8"">
                <style>
                    .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                    .security-notice {{ background-color: #fff4f4; border: 1px solid #f8d7da; padding: 15px; margin: 20px 0; border-radius: 4px; color: #721c24; font-size: 14px; }}
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
                                            Паролата Ви беше променена
                                        </span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 40px 30px;"">
                                        <div class=""content-text"">
                                            <p>Здравейте, {user.Name?.Split(" ")[0]},</p>
                                            <p>Паролата за Вашия профил в уебсайта на <strong>{WebsiteName}</strong> беше успешно променена на {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC.</p>
                                        </div>

                                        <div class=""security-notice"">
                                            <strong>Не сте били Вие?</strong><br>
                                            Ако не сте инициирали тази промяна, моля свържете се с нашата поддръжка незабавно или нулирайте паролата си през входната форма.
                                        </div>

                                        <p style=""font-size: 14px; color: #999; margin-top: 30px;"">
                                            С уважение,<br>
                                            Екипът на {WebsiteName}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style=""padding: 20px; background-color: #fbf3fb; text-align: center; font-size: 12px; color: #616161; border-radius: 0 0 8px 8px;"">
                                        <p style=""margin: 0;"">Това е автоматично съобщение. Моля, не отговаряйте на него.</p>
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
            "Паролата Ви беше променена",
            passwordChangedBody,
            true);

        return RedirectToPage();
    }
}