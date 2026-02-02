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

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            TempData["RedirectSource"] = "ForgotPassword";

            ApplicationUser user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return RedirectToPage("./ForgotPasswordConfirmation", new { email = Input.Email }); // Don't reveal that the user does not exist or is not confirmed

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            string resetPasswordBody = $@"
                <!DOCTYPE html>
                <html lang=""bg"">
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                        .content-text {{ line-height: 1.6; color: #181717; font-size: 16px; }}
                        .button-container {{ text-align: center; margin: 30px 0; }}
                        .button {{ 
                            background-color: #3a053a; 
                            color: #ffffff !important; 
                            padding: 12px 25px; 
                            text-decoration: none; 
                            border-radius: 5px; 
                            font-weight: bold; 
                            display: inline-block;
                        }}
                        .small-text {{ font-size: 12px; color: #999; margin-top: 20px; word-break: break-all; }}
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
                                                Възстановяване на парола
                                            </span>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 40px 30px;"">
                                            <div class=""content-text"">
                                                <p>Здравейте,</p>
                                                <p>Получихме заявка за нулиране на паролата за Вашия акаунт в сайта на <strong>{WebsiteName}</strong>. Можете да го направите, като кликнете върху бутона по-долу:</p>
                                                
                                                <div class=""button-container"">
                                                    <a href=""{callbackUrl}"" class=""button"">Нулиране на паролата</a>
                                                </div>
                                                
                                                <p>Ако бутонът не работи, копирайте и поставете следния линк във Вашия браузър:</p>
                                                <p class=""small-text"">{callbackUrl}</p>
                                                
                                                <p>Ако не сте заявявали промяна на паролата, можете безопасно да игнорирате този имейл. Вашата парола ще остане непроменена.</p>
                                            </div>

                                            <p style=""font-size: 14px; color: #999; margin-top: 30px;"">
                                                С уважение,<br>
                                                Екипът на {WebsiteName}
                                            </p>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 20px; background-color: #fbf3fb; text-align: center; font-size: 12px; color: #616161; border-radius: 0 0 8px 8px;"">
                                            <p style=""margin: 0;"">Този имейл е изпратен автоматично във връзка със сигурността на Вашия профил.</p>
                                            <p style=""margin: 5px 0 0 0;"">&copy; {DateTime.UtcNow.Year} {WebsiteName}</p>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";


            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var mailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var userProvider = scope.ServiceProvider.GetRequiredService<IEmailUserProvider>();

                    await mailService.SendEmailAsync(
                        userProvider.GetNotificationsUser(),
                        Input.Email,
                        "Възстановяване на парола",
                        resetPasswordBody,
                        true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex); // TODO: Log this exception
                }
            });

            return RedirectToPage("./ForgotPasswordConfirmation", new { email = Input.Email });
        }
    }
}
