// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.EntityConstraints.ApplicationUser;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceProvider _serviceProvider;

    public ResetPasswordModel(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
    {
        _userManager = userManager;
        _serviceProvider = serviceProvider;
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
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(PasswordMaxLength, ErrorMessage = "{0} трябва да бъде поне {2} и максимум {1} символа.",
            MinimumLength = PasswordMinLength)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        public string Code { get; set; }

    }

    public IActionResult OnGet(string code = null)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        else
        {
            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        ApplicationUser user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null) // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");

        IdentityResult result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
        {
            await _userManager.UpdateSecurityStampAsync(user);

            if (user.Email is not null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using IServiceScope scope = _serviceProvider.CreateScope();
                        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        IEmailUserProvider emailUserProvider =
                            scope.ServiceProvider.GetRequiredService<IEmailUserProvider>();

                        string successBody = $@"
                            <!DOCTYPE html>
                            <html lang=""bg"">
                            <head>
                                <meta charset=""UTF-8"">
                                <style>
                                    .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                                    .warning-box {{ background-color: #fff4f4; border: 1px solid #f8d7da; padding: 15px; margin: 20px 0; color: #721c24; font-size: 14px; border-radius: 4px; }}
                                </style>
                            </head>
                            <body style=""margin: 0; padding: 0; background-color: #fcfcfc; font-family: 'Segoe UI', Arial, sans-serif;"">
                                <table role=""presentation"" width=""100%"" style=""background-color: #fcfcfc;"">
                                    <tr>
                                        <td align=""center"" style=""padding: 20px 0;"">
                                            <table role=""presentation"" width=""600"" style=""background-color: #ffffff; border: 1px solid #f0f0f0; border-radius: 8px;"">
                                                <tr>
                                                    <td style=""padding: 20px; background-color: #3a053a; border-radius: 8px 8px 0 0; text-align: center;"">
                                                        <span style=""color: #fcfcfc; font-size: 20px; font-weight: bold;"">Успешна промяна на паролата</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style=""padding: 40px 30px;"">
                                                        <div class=""content-text"">
                                                            <p>Здравейте, {user.Name},</p>
                                                            <p>Паролата за Вашия профил в <strong>{WebsiteName}</strong> беше успешно променена преди малко.</p>
                                                            <p>Ако сте направили тази промяна лично, можете да игнорирате този имейл.</p>
                                                        </div>
                                                        <div class=""warning-box"">
                                                            <strong>Важно:</strong> Ако не сте инициирали тази промяна, моля свържете се с нас веднага, тъй като сигурността на Вашия профил може да е застрашена.
                                                        </div>
                                                        <p style=""font-size: 14px; color: #999; margin-top: 30px;"">
                                                            С уважение,<br>Екипът на {WebsiteName}
                                                        </p>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </body>
                            </html>";

                        await emailService.SendEmailAsync(
                            emailUserProvider.GetNotificationsUser(),
                            user.Email,
                            "Вашата парола беше променена",
                            successBody,
                            true
                        );
                    }
                    catch (Exception ex)
                    {
                        // TODO Log the exception (use your logger here)
                        Console.WriteLine($"Email background task failed: {ex.Message}");
                    }
                });
            }

            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (IdentityError error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}