using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.ViewModels.Home;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using Microsoft.AspNetCore.Identity;

namespace MHAuthorWebsite.Web.Controllers;

public class HomeController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILegalDocumentsService _legalDocumentsService;

    public HomeController(UserManager<ApplicationUser> userManager, ILegalDocumentsService legalDocumentsService)
    {
        _userManager = userManager;
        _legalDocumentsService = legalDocumentsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index() => View();

    [HttpGet]
    [AllowAnonymous]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> PrivacyPolicy()
    {
        Core.Dtos.Legal.LegalDocumentDto document = await _legalDocumentsService
            .GetLatestDocumentReadonlyAsync(LegalDocumentType.PrivacyPolicy);
        return View(document);
    }

    [HttpGet]
    [AllowAnonymous]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> TermsOfService()
    {
        Core.Dtos.Legal.LegalDocumentDto document = await _legalDocumentsService
            .GetLatestDocumentReadonlyAsync(LegalDocumentType.TermsOfService);
        return View(document);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> UnsubscribeMarketing([FromQuery] string email, [FromQuery] string token)
    {
        MarketingUnsubscribeResultViewModel model = new();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            model.IsSuccess = false;
            model.Title = "Невалиден линк";
            model.Message = "Линкът за отписване е невалиден или непълен.";
            return View(model);
        }

        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsMarketingSubscribed || string.IsNullOrWhiteSpace(user.MarketingUnsubscribeToken))
        {
            model.IsSuccess = false;
            model.Title = "Отписването е неуспешно";
            model.Message = "Този абонамент вече е неактивен или линкът е изтекъл.";
            return View(model);
        }

        if (!string.Equals(user.MarketingUnsubscribeToken, token, StringComparison.Ordinal))
        {
            model.IsSuccess = false;
            model.Title = "Невалиден токен";
            model.Message = "Линкът за отписване е невалиден.";
            return View(model);
        }

        user.IsMarketingSubscribed = false;
        user.MarketingUnsubscribedOn = DateTime.UtcNow;
        user.MarketingUnsubscribeToken = null;
        user.MarketingUnsubscribeTokenCreatedOn = null;

        IdentityResult updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            model.IsSuccess = false;
            model.Title = "Отписването е неуспешно";
            model.Message = "Възникна технически проблем при обработката на заявката.";
            return View(model);
        }

        model.IsSuccess = true;
        model.Title = "Успешно отписване";
        model.Message = "Вече няма да получавате маркетинг съобщения от нас.";

        return View(model);
    }
}
