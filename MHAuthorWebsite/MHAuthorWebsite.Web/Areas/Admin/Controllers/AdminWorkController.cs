using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Admin.Dto.Work;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.Utils.Extensions;
using MHAuthorWebsite.Web.ViewModels.Admin.Work;
using Microsoft.AspNetCore.Mvc;
using static MHAuthorWebsite.GCommon.EntityConstraints.Work;
using static MHAuthorWebsite.Web.Utils.Helpers.EditorHelper;
using static MHAuthorWebsite.Web.Utils.Mappers.ImageMapper;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminWorkController : AdminBaseController
{
    private readonly IAdminWorkService _adminWorkService;

    public AdminWorkController(IAdminWorkService adminWorkService) => _adminWorkService = adminWorkService;

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor)]
    public IActionResult Add() => View();

    [HttpPost]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> Add(AddWorkForm model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.CoverImage.ExceedsCloudinarySizeLimit())
        {
            ModelState.AddModelError(nameof(model.CoverImage), ImageValidationExtensions.GetCloudinarySizeLimitValidationMessage());
            return View(model);
        }

        string delta = model.Content;
        string plainText = ExtractPlainTextFromQuillDelta(delta);

        if (plainText.Length < ContentTextMinLength)
        {
            ModelState.AddModelError(nameof(model.Content), $"Съдържанието не трябва да е по-кратко от {ContentTextMinLength} символа.");
            return View(model);
        }

        if (plainText.Length > ContentTextMaxLength)
        {
            ModelState.AddModelError(nameof(model.Content), $"Съдържанието не трябва да надвишава {ContentTextMaxLength} символа.");
            return View(model);
        }

        if (delta.Length > ContentDeltaMaxLength)
        {
            ModelState.AddModelError(nameof(model.Content), "Съдържанието е прекалено голямо.");
            return View(model);
        }

        AddWorkDto dto = new()
        {
            Title = model.Title,
            Content = model.Content,
            IsPublic = model.IsPublic,
            CoverImage = await MapIFormFileToUploadImageRequestDtoAsync(model.CoverImage, HttpContext.RequestAborted)
        };

        ServiceResult result = await _adminWorkService.AddWorkAsync(dto);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, "Възникна грешка при добавянето на творба.");
            return View(model);
        }

        return RedirectToAction("Index", "Work", new { area = "" });
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> Edit(Guid id)
    {
        ServiceResult<EditWorkDto> result = await _adminWorkService.GetWorkForEditReadonlyAsync(id);

        if (!result.Found) return NotFound();
        if (!result.Success) return StatusCode(500);

        EditWorkDto dto = result.Result!;
        EditWorkFormViewModel viewModel = new()
        {
            Id = dto.Id,
            Title = dto.Title,
            Content = dto.Content,
            CurrentCoverImageUrl = dto.CurrentCoverImageUrl,
            IsPublic = dto.IsPublic
        };

        return View(viewModel);
    }

    [HttpPost]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> Edit(EditWorkFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.NewCoverImage != null && model.NewCoverImage.ExceedsCloudinarySizeLimit())
        {
            ModelState.AddModelError(nameof(model.NewCoverImage), ImageValidationExtensions.GetCloudinarySizeLimitValidationMessage());
            return View(model);
        }

        string delta = model.Content;
        string plainText = ExtractPlainTextFromQuillDelta(delta);

        if (plainText.Length < ContentTextMinLength)
        {
            ModelState.AddModelError(nameof(model.Content), $"Съдържанието не трябва да е по-кратко от {ContentTextMinLength} символа.");
            return View(model);
        }

        if (plainText.Length > ContentTextMaxLength)
        {
            ModelState.AddModelError(nameof(model.Content), $"Съдържанието не трябва да надвишава {ContentTextMaxLength} символа.");
            return View(model);
        }

        if (delta.Length > ContentDeltaMaxLength)
        {
            ModelState.AddModelError(nameof(model.Content), "Съдържанието е прекалено голямо.");
            return View(model);
        }

        EditWorkDto dto = new()
        {
            Id = model.Id,
            Title = model.Title,
            Content = model.Content,
            IsPublic = model.IsPublic,
            CurrentCoverImageUrl = model.CurrentCoverImageUrl,
            NewCoverImage = model.NewCoverImage != null
                ? await MapIFormFileToUploadImageRequestDtoAsync(model.NewCoverImage, HttpContext.RequestAborted)
                : null
        };

        ServiceResult result = await _adminWorkService.UpdateWorkAsync(dto);

        if (!result.Success)
        {
            if (result.Found == false) return NotFound();
            ModelState.AddModelError(string.Empty, "Възникна грешка при редактирането на творба.");
            return View(model);
        }

        return RedirectToAction("Index", "Work", new { area = "" });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        ServiceResult result = await _adminWorkService.DeleteWorkAsync(id);

        if (!result.Success)
            return StatusCode(500);

        return RedirectToAction("Index", "Work", new { area = "" });
    }

    [HttpPost]
    public async Task<IActionResult> TogglePublicity(Guid id)
    {
        ServiceResult result = await _adminWorkService.ToggleWorkPublicityAsync(id);
        if (!result.Success)
            return StatusCode(500);

        return RedirectToAction("Index", "Work", new { area = "" });
    }
}
