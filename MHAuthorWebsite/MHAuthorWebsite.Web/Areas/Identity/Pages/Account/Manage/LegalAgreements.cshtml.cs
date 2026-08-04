using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage;

[SecurityHeaders(CspFeature.Notifications)]
public class LegalAgreementsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILegalDocumentsService _legalDocumentsService;

    public LegalAgreementsModel(UserManager<ApplicationUser> userManager, ILegalDocumentsService legalDocumentsService)
    {
        _userManager = userManager;
        _legalDocumentsService = legalDocumentsService;
    }

    [BindProperty]
    public bool AcceptLatestDocuments { get; set; }

    public ICollection<LegalDocumentDto> PendingDocuments { get; set; } = Array.Empty<LegalDocumentDto>();

    public bool ShowPrompt { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] int? prompt = null)
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToPage("/Account/Login");

        PendingDocuments = await _legalDocumentsService.GetPendingLatestDocumentsReadonlyAsync(user.Id);
        ShowPrompt = prompt == 1 && PendingDocuments.Any();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ApplicationUser? user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToPage("/Account/Login");

        if (!AcceptLatestDocuments)
        {
            ModelState.AddModelError(nameof(AcceptLatestDocuments), "Трябва да приемете Политиката за поверителност и Общите условия.");
            PendingDocuments = await _legalDocumentsService.GetPendingLatestDocumentsReadonlyAsync(user.Id);
            return Page();
        }

        await _legalDocumentsService.AcceptLatestDocumentsAsync(user.Id);

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToAction("Index", "Home", new { area = "" });
    }
}
