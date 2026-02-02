// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceProvider _serviceProvider;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
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

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            ApplicationUser user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is null || user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Успешно е изпратен е имейл за потвърждение! Моля, проверете вашата поща!");
                return Page();
            }

            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId, code },
                protocol: Request.Scheme);

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    IEmailUserProvider emailUserProvider = scope.ServiceProvider.GetRequiredService<IEmailUserProvider>();

                    await emailService.SendEmailAsync(
                        emailUserProvider.GetNotificationsUser(),
                        Input.Email,
                        "Потвърдете Вашият имейл адрес",
                        $@"<!DOCTYPE html>
                            <html lang=""bg"">
                            <head>
                                <meta charset=""UTF-8"">
                                <style>
                                    .btn-link:hover {{ background-color: #2767e7 !important; }}
                                </style>
                            </head>
                            <body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #fcfcfc; color: #181717;"">
                                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; margin: 20px auto; border: 1px solid #f8dff8; border-radius: 15px; overflow: hidden; background-color: #ffffff;"">
                                    <tr>
                                        <td style=""padding: 30px 20px; text-align: center; background-color: #3a053a;"">
                                            <h1 style=""color: #fcfcfc; margin: 0; font-size: 28px; letter-spacing: 1px;"">Добре дошли!</h1>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 40px 30px; text-align: center;"">
                                            <h2 style=""color: #3a053a; font-size: 22px; margin-bottom: 20px;"">Потвърдете вашия имейл</h2>
                                            <p style=""color: #616161; font-size: 16px; line-height: 1.6; margin-bottom: 30px;"">
                                                Благодарим Ви за регистрацията. За да активирате вашия акаунт и да получите достъп до всички функции, моля, потвърдете вашия имейл адрес чрез бутона по-долу.
                                            </p>
                                            <a href=""{HtmlEncoder.Default.Encode(callbackUrl!)}"" 
                                               style=""background-color: #3a053a; color: #fcfcfc; padding: 15px 30px; text-decoration: none; font-size: 16px; font-weight: bold; border-radius: 10px; display: inline-block;"">
                                                Кликнете тук за потвърждение
                                            </a>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 20px; background-color: #fbf3fb; text-align: center; color: #616161; font-size: 12px;"">
                                            <p style=""margin: 5px 0;"">Ако не сте правили тази регистрация, можете безопасно да игнорирате този имейл.</p>
                                            <hr style=""border: 0; border-top: 1px solid #f8dff8; margin: 15px 0;"">
                                            <p style=""margin: 5px 0;"">&copy; {DateTime.UtcNow.Year} {WebsiteName}. Всички права запазени.</p>
                                        </td>
                                    </tr>
                                </table>
                            </body>
                            </html>",
                        true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex); // TODO: Log this exception
                }
            });

            ModelState.AddModelError(string.Empty, "Успешно е изпратен е имейл за потвърждение! Моля, проверете вашата поща!");
            return Page();
        }
    }
}
