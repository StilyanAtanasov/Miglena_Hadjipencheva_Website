// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.EmailConfiguration.Contracts;
using MHAuthorWebsite.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IEmailUserProvider _emailUserProvider;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, IEmailService emailService, IEmailUserProvider emailUserProvider)
        {
            _userManager = userManager;
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

            await _emailService.SendEmailAsync(
                _emailUserProvider.GetNotificationsUser(),
                Input.Email,
                "Потвърдете Вашият имейл адрес",
                $@"<!DOCTYPE html>
                 <html>
                   <body>
                      <p>Моля, потвърдете Вашият акаунт, като
                         <a href=""{HtmlEncoder.Default.Encode(callbackUrl!)}"">кликнете тук</a>.
                       </p>
                    </body>
                 </html>",
                true);

            ModelState.AddModelError(string.Empty, "Успешно е изпратен е имейл за потвърждение! Моля, проверете вашата поща!");
            return Page();
        }
    }
}
