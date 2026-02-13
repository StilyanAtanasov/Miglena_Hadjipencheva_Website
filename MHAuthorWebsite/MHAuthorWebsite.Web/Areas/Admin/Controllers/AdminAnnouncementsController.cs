using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.Announcements;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Admin.Announcements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static MHAuthorWebsite.GCommon.ApplicationRules.AnnouncementsBoard;
using static MHAuthorWebsite.GCommon.EntityConstraints.Announcement;
using static MHAuthorWebsite.Web.Utils.Helpers.EditorHelper;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminAnnouncementsController : AdminBaseController
{
    private readonly IAdminAnnouncementsService _adminAnnouncementsService;

    public AdminAnnouncementsController(IAdminAnnouncementsService adminAnnouncementsService)
        => _adminAnnouncementsService = adminAnnouncementsService;

    [HttpGet]
    [SecurityHeaders(CspFeature.Notifications)]
    public async Task<IActionResult> AnnouncementsBoard([FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;

        int announcementsCount = await _adminAnnouncementsService.GetAnnouncementsCountReadonlyAsync(search);
        if (announcementsCount > 0 && Math.Ceiling((double)announcementsCount / AnnouncementsPerPage) < page)
            return NotFound();

        ICollection<AnnouncementListItemDto> announcements =
            await _adminAnnouncementsService.GetAnnouncementsPagedReadonlyAsync(page, search);

        ICollection<AnnouncementListItemViewModel> viewModels = announcements
            .Select(item => new AnnouncementListItemViewModel
            {
                Id = item.Id,
                Subject = item.Subject,
                MessagePreview = item.MessagePreview,
                RecipientGroupLabel = item.RecipientGroup.GetDisplayName(),
                RecipientCount = item.RecipientCount,
                CreatedOn = item.CreatedOn,
                AdminName = item.AdminName
            })
            .ToArray();

        ViewBag.AnnouncementsCount = announcementsCount;
        ViewBag.CurrentSearch = search ?? string.Empty;

        if (HttpContext.Request.Headers.Any(h => h.Key == "X-Requested-With" && h.Value == "XMLHttpRequest"))
            return PartialView("_AnnouncementsGridPartial", viewModels);

        return View(viewModels);
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> AnnouncementDetails(Guid id)
    {
        ServiceResult<AnnouncementDetailsDto> result = await _adminAnnouncementsService.GetAnnouncementDetailsReadonlyAsync(id);
        if (!result.Found) return NotFound();

        AnnouncementDetailsDto dto = result.Result!;
        AnnouncementDetailsViewModel viewModel = new()
        {
            Id = dto.Id,
            Subject = dto.Subject,
            MessageDelta = dto.MessageDelta,
            RecipientGroupLabel = dto.RecipientGroup.GetDisplayName(),
            AdditionalRecipients = dto.AdditionalRecipients,
            RecipientCount = dto.RecipientCount,
            CreatedOn = dto.CreatedOn,
            AdminName = dto.AdminName
        };

        return View(viewModel);
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor)]
    public IActionResult AddAnnouncement()
    {
        PopulateRecipientGroups();
        return View(new AddAnnouncementFormViewModel());
    }

    [HttpPost]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> AddAnnouncement(AddAnnouncementFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            PopulateRecipientGroups();
            return View(model);
        }

        string plainText = ExtractPlainTextFromQuillDelta(model.MessageDelta).Trim();

        if (plainText.Length < MessageTextMinLength)
            ModelState.AddModelError(nameof(model.MessageDelta), $"Съобщението не трябва да е по-кратко от {MessageTextMinLength} символа.");

        if (plainText.Length > MessageTextMaxLength)
            ModelState.AddModelError(nameof(model.MessageDelta), $"Съобщението не трябва да надвишава {MessageTextMaxLength} символа.");

        if (model.MessageDelta.Length > MessageDeltaMaxLength)
            ModelState.AddModelError(nameof(model.MessageDelta), "Съдържанието е прекалено голямо.");

        if (!ModelState.IsValid)
        {
            PopulateRecipientGroups();
            return View(model);
        }

        CreateAnnouncementDto dto = new()
        {
            Subject = model.Subject,
            MessageDelta = model.MessageDelta,
            RecipientGroup = model.RecipientGroup,
            AdditionalRecipients = model.AdditionalRecipients
        };

        ServiceResult result = await _adminAnnouncementsService.CreateAnnouncementAsync(dto, GetUserId()!);
        if (!result.Success)
        {
            foreach ((string key, string value) in result.Errors)
                ModelState.AddModelError(key, value);

            PopulateRecipientGroups();
            return View(model);
        }

        return RedirectToAction(nameof(AnnouncementsBoard));
    }

    private void PopulateRecipientGroups()
    {
        ViewBag.RecipientGroups = Enum.GetValues(typeof(AnnouncementRecipientGroup))
            .Cast<AnnouncementRecipientGroup>()
            .Select(group => new SelectListItem
            {
                Value = ((int)group).ToString(),
                Text = group.GetDisplayName()
            })
            .ToArray();
    }
}

