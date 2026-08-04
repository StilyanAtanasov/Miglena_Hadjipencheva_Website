// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using MHAuthorWebsite.Core.Configuration.EmailConfiguration.Contracts;
using MHAuthorWebsite.Core.Configuration.Security;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Web.Common.Localization;
using MHAuthorWebsite.Web.Utils.Contracts;
using MHAuthorWebsite.Web.Utils.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using static MHAuthorWebsite.GCommon.ApplicationRules.Application;
using static MHAuthorWebsite.GCommon.EntityConstraints.ApplicationUser;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILegalDocumentsService _legalDocumentsService;
    private readonly IRecaptchaValidationService _recaptchaValidationService;
    private readonly RecaptchaSettings _recaptchaSettings;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IServiceProvider serviceProvider,
        ILegalDocumentsService legalDocumentsService,
        IRecaptchaValidationService recaptchaValidationService,
        IOptions<RecaptchaSettings> recaptchaSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _legalDocumentsService = legalDocumentsService;
        _recaptchaValidationService = recaptchaValidationService;
        _recaptchaSettings = recaptchaSettings.Value;
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
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public string RecaptchaV2SiteKey => _recaptchaSettings.V2SiteKey;

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
        /// 
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
        [Display(Name = "Име и фамилия")]
        [RegularExpression(@"^\S+\s+\S+$", ErrorMessage = "Моля, въведете име и фамилия!")]
        [StringLength(NameMaxLength, MinimumLength = NameMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
        [EmailAddress]
        [Display(Name = "Имейл")]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "Required")]
        [StringLength(PasswordMaxLength, MinimumLength = PasswordMinLength, ErrorMessageResourceType = typeof(ValidationMessages), ErrorMessageResourceName = "StringLength")]
        [DataType(DataType.Password)]
        [Display(Name = "Парола")]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Потвърди парола")]
        [Compare("Password", ErrorMessage = "Двете въведени пароли не съвпадат.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Съгласен съм с Политиката за поверителност и Общите условия")]
        [MustBeTrue(ErrorMessage = "Трябва да приемете Политиката за поверителност и Общите условия.")]
        public bool HasAcceptedLegalDocuments { get; set; }

        [Display(Name = "Съгласен съм да получавам маркетинг съобщения")]
        public bool IsMarketingSubscribed { get; set; }
    }


    public async Task OnGetAsync(string returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            Response.Redirect("/Home/Index/");
            return;
        }

        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (User.Identity?.IsAuthenticated ?? false) return Forbid();
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            string recaptchaToken = Request.Form["g-recaptcha-response"].ToString();
            RecaptchaValidationResult v2VerificationResult = await _recaptchaValidationService.VerifyV2Async(recaptchaToken, cancellationToken);
            if (!v2VerificationResult.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Моля, потвърдете, че не сте робот.");
                return Page();
            }

            ApplicationUser user = new()
            {
                Name = Input.Name,
                Email = Input.Email,
                NormalizedEmail = Input.Email.ToUpper(),
                UserName = Input.Email,
                NormalizedUserName = Input.Email.ToUpper(),
                RegisteredOn = DateTime.UtcNow,
                LastActive = DateTime.UtcNow,
                HasAcceptedPrivacyPolicy = Input.HasAcceptedLegalDocuments,
                PrivacyPolicyAcceptedOn = DateTime.UtcNow,
                PrivacyPolicyVersion = PrivacyPolicyCurrentVersion,
                IsMarketingSubscribed = Input.IsMarketingSubscribed,
                MarketingSubscribedOn = Input.IsMarketingSubscribed ? DateTime.UtcNow : null,
                MarketingUnsubscribeToken = Input.IsMarketingSubscribed ? GenerateOneTimeToken() : null,
                MarketingUnsubscribeTokenCreatedOn = Input.IsMarketingSubscribed ? DateTime.UtcNow : null
            };

            IdentityResult result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                string userId = await _userManager.GetUserIdAsync(user);
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

#pragma warning disable CS4014
                SendEmailConfirmationAsync(userId, code, returnUrl);
#pragma warning restore CS4014

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });

                await _legalDocumentsService.AcceptLatestDocumentsAsync(user.Id);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private static string GenerateOneTimeToken()
        => $"{Guid.NewGuid():N}{Guid.NewGuid():N}";

    private Task SendEmailConfirmationAsync(string userId, string code, string returnUrl)
    {
        string callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { userId, code },
            protocol: Request.Scheme);

        string targetEmail = Input.Email;

        _ = Task.Run(async () =>
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            ILogger<RegisterModel> logger = scope.ServiceProvider.GetRequiredService<ILogger<RegisterModel>>();

            try
            {
                IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                IEmailUserProvider emailUserProvider = scope.ServiceProvider.GetRequiredService<IEmailUserProvider>();

                await emailService.SendEmailAsync(
                    emailUserProvider.GetNotificationsUser(),
                    targetEmail,
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
                logger.LogError($"Error sending email confirmation to {targetEmail}: {ex.Message}");
                logger.LogError(ex.ToString());
            }
        });

        return Task.CompletedTask;
    }
}
