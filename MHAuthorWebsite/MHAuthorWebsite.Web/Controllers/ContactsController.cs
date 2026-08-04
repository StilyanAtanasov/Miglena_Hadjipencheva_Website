using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Contacts;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Contracts;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Contacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class ContactsController : BaseController
{
    private const string AutomationBlockMessage = "Вашата активност наподобява автоматизирано поведение. Моля, опитайте отново.";
    private const string ManualCaptchaMessage = "Моля, потвърдете ръчно, че не сте робот.";

    private readonly IContactsService _contactsService;
    private readonly IRecaptchaValidationService _recaptchaValidationService;

    public ContactsController(
        IContactsService contactsService,
        IRecaptchaValidationService recaptchaValidationService)
    {
        _contactsService = contactsService;
        _recaptchaValidationService = recaptchaValidationService;
    }

    [SecurityHeaders(CspFeature.Notifications | CspFeature.Recaptcha)]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SendEmail(ContactFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest();

        RecaptchaValidationResult v3VerificationResult = await _recaptchaValidationService.VerifyV3Async(
            model.RecaptchaV3Token,
            "contact_request",
            cancellationToken: cancellationToken);

        if (!v3VerificationResult.IsSuccess)
        {
            if (v3VerificationResult.RequiresManualChallenge)
            {
                bool hasManualToken = !string.IsNullOrWhiteSpace(model.RecaptchaV2Token);
                RecaptchaValidationResult v2VerificationResult =
                    await _recaptchaValidationService.VerifyV2Async(model.RecaptchaV2Token, cancellationToken);

                if (!v2VerificationResult.IsSuccess)
                {
                    if (hasManualToken) return BadRequest(new { message = AutomationBlockMessage });
                    return StatusCode(StatusCodes.Status428PreconditionRequired, new { message = ManualCaptchaMessage });
                }
            }
            else return BadRequest(new { message = AutomationBlockMessage });
        }

        SendContactMessageDto dto = new SendContactMessageDto
        {
            Name = model.Name,
            Email = model.Email,
            Subject = model.Subject,
            Message = model.Message
        };

        ServiceResult sr = await _contactsService.SendContactMessageAsync(dto, GetUserId());
        if (!sr.HasPermission) return Forbid();

        return Ok();
    }
}
