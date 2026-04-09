// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MHAuthorWebsite.Core.Configuration.EmailConfiguration;
using MHAuthorWebsite.Core.Contracts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;

    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string Username { get; set; }

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
        [Required]
        [EmailAddress]
        [Display(Name = "Имейл")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Телефонен номер")]
        public string PhoneNumber { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        var userName = await _userManager.GetUserNameAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        var email = await _userManager.GetEmailAsync(user);

        Username = userName;

        Input = new InputModel
        {
            Email = email,
            PhoneNumber = phoneNumber
        };
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
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
            await LoadAsync(user);
            return Page();
        }

        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        var email = await _userManager.GetEmailAsync(user);
        if (Input.Email != email)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.Email);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, email = Input.Email, code = code },
                protocol: Request.Scheme);

            user.PendingEmail = Input.Email;
            await _userManager.UpdateAsync(user);

            // Send notifications
            await _emailService.SendEmailAsync(
                _emailSettings.NotificationsEmailUser,
                email,
                "Промяна на имейл адрес",
                $"Здравейте, получен е опит за смяна на вашия имейл адрес с {Input.Email}. Ако това не сте вие, моля свържете се с нас.",
                false);

            await _emailService.SendEmailAsync(
                _emailSettings.NotificationsEmailUser,
                Input.Email,
                "Потвърдете вашия нов имейл адрес",
                $"Моля потвърдете вашия нов имейл адрес като <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>кликнете тук</a>.",
                true);

            StatusMessage = "На стария и на новия ви имейл адрес са изпратени съобщения. Моля, потвърдете промяната чрез линка в новия ви имейл.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Вашият профил беше успешно обновен.";
        return RedirectToPage();
    }
}