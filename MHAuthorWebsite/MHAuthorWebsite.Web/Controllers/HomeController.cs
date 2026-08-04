using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Seo;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Globalization;
using System.Xml.Linq;

namespace MHAuthorWebsite.Web.Controllers;

public class HomeController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILegalDocumentsService _legalDocumentsService;
    private readonly ISitemapService _sitemapService;

    public HomeController(UserManager<ApplicationUser> userManager, ILegalDocumentsService legalDocumentsService,
        ISitemapService sitemapService)
    {
        _userManager = userManager;
        _legalDocumentsService = legalDocumentsService;
        _sitemapService = sitemapService;
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

    [HttpGet("/faq")]
    [AllowAnonymous]
    public IActionResult Faq() => View();

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

    [HttpGet("/robots.txt")]
    [AllowAnonymous]
    public IActionResult RobotsTxt()
    {
        string? sitemapUrl = Url.ActionLink(nameof(SitemapXml), "Home", values: null, protocol: Request.Scheme);

        List<string> lines = new()
        {
            "User-agent: *",
            "Allow: /",
            "Disallow: /Admin",
            "Disallow: /Identity",
            "Disallow: /Order",
            "Disallow: /Cart",
            "Disallow: /Product/LikedProducts"
        };

        if (!string.IsNullOrWhiteSpace(sitemapUrl)) lines.Add($"Sitemap: {sitemapUrl}");

        return Content(string.Join('\n', lines), "text/plain; charset=utf-8");
    }

    [HttpGet("/sitemap.xml")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "SitemapPolicy")]
    public async Task<IActionResult> SitemapXml()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        IReadOnlyCollection<SitemapUrlDto> urls = await _sitemapService.GetPublicSitemapUrlsAsync();

        IEnumerable<XElement> urlElements = urls.Select(url =>
        {
            List<object> children = new()
            {
                new XElement(ns + "loc", GetAbsoluteUrl(url.RelativeUrl))
            };

            if (url.LastModifiedUtc.HasValue)
                children.Add(new XElement(ns + "lastmod", url.LastModifiedUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")));

            if (!string.IsNullOrWhiteSpace(url.ChangeFrequency))
                children.Add(new XElement(ns + "changefreq", url.ChangeFrequency));

            if (url.Priority.HasValue)
                children.Add(new XElement(ns + "priority", url.Priority.Value.ToString("0.0", CultureInfo.InvariantCulture)));

            return new XElement(ns + "url", children);
        });

        XDocument sitemap = new(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "urlset", urlElements));

        return Content(sitemap.Declaration + Environment.NewLine + sitemap, "application/xml; charset=utf-8");
    }

    private string GetAbsoluteUrl(string relativeUrl)
    {
        string normalized = relativeUrl.StartsWith('/') ? relativeUrl : $"/{relativeUrl}";
        return $"{Request.Scheme}://{Request.Host}{normalized}";
    }
}
