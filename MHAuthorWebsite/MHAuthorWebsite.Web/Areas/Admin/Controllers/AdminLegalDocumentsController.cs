using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Extensions;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.LegalDocuments;
using MHAuthorWebsite.Core.Dtos.Legal;
using MHAuthorWebsite.Core.Models.Enums;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Admin.LegalDocuments;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminLegalDocumentsController : AdminBaseController
{
    private readonly IAdminLegalDocumentsService _adminLegalDocumentsService;

    public AdminLegalDocumentsController(IAdminLegalDocumentsService adminLegalDocumentsService)
        => _adminLegalDocumentsService = adminLegalDocumentsService;

    [HttpGet]
    public async Task<IActionResult> Documents()
    {
        ICollection<AdminLegalDocumentSummaryDto> docs = await _adminLegalDocumentsService.GetLatestDocumentsSummaryReadonlyAsync();
        ICollection<LegalDocumentSummaryViewModel> vm = docs
            .Select(d => new LegalDocumentSummaryViewModel
            {
                DocumentType = d.DocumentType.GetDisplayName(),
                Title = d.Title,
                Version = Math.Max(1, d.Version),
                CreatedOn = d.CreatedOn,
                EditAction = d.DocumentType == LegalDocumentType.PrivacyPolicy ? nameof(EditPrivacyPolicy) : nameof(EditTermsOfService)
            })
            .ToArray();

        ViewData["CurrentPage"] = GCommon.ApplicationRules.Pages.Admin.LegalDocuments;
        ViewData["RequireTimeZoneManager"] = true;
        return View(vm);
    }

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> EditPrivacyPolicy()
        => await EditDocumentAsync(LegalDocumentType.PrivacyPolicy);

    [HttpGet]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> EditTermsOfService()
        => await EditDocumentAsync(LegalDocumentType.TermsOfService);

    [HttpPost]
    [SecurityHeaders(CspFeature.Editor)]
    public async Task<IActionResult> SaveDocument(EditLegalDocumentViewModel model)
    {
        model.DocumentTypeDisplayName = model.DocumentType.GetDisplayName();
        ModelState.Remove(nameof(EditLegalDocumentViewModel.DocumentTypeDisplayName));
        ModelState.ClearValidationState(string.Empty);
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            foreach (var kvp in ModelState)
            {
                if (kvp.Value.Errors.Count > 0)
                {
                    Console.WriteLine(
                        "ModelState error on {Key}: {Errors}",
                        kvp.Key,
                        string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))
                    );
                }
            }

            EditLegalDocumentViewModel hydratedModel = await HydrateEditModelAsync(model);
            ViewData["CurrentPage"] = GCommon.ApplicationRules.Pages.Admin.LegalDocuments;
            return View("EditDocument", hydratedModel);
        }

        AdminLegalDocumentUpdateDto dto = new()
        {
            DocumentType = model.DocumentType,
            Title = model.Title,
            Nodes = model.Nodes
                .Select(n => new LegalDocumentNodeDto
                {
                    Number = n.Number,
                    Title = n.Title,
                    ContentDelta = n.ContentDelta
                })
                .ToArray()
        };

        ServiceResult<int> result = await _adminLegalDocumentsService.SaveNewDocumentVersionAsync(dto, GetUserId()!);
        if (!result.Success)
        {
            foreach ((string key, string value) in result.Errors)
                ModelState.AddModelError(key, value);

            EditLegalDocumentViewModel hydratedModel = await HydrateEditModelAsync(model);
            ViewData["CurrentPage"] = GCommon.ApplicationRules.Pages.Admin.LegalDocuments;
            return View("EditDocument", hydratedModel);
        }

        return RedirectToAction(nameof(Documents));
    }

    private async Task<IActionResult> EditDocumentAsync(LegalDocumentType type)
    {
        ServiceResult<LegalDocumentDto> result = await _adminLegalDocumentsService.GetLatestDocumentForEditReadonlyAsync(type);
        if (!result.Success || !result.Found) return NotFound();

        LegalDocumentDto dto = result.Result!;
        EditLegalDocumentViewModel model = new()
        {
            DocumentType = dto.DocumentType,
            DocumentTypeDisplayName = dto.DocumentType.GetDisplayName(),
            CurrentVersion = Math.Max(1, dto.Version),
            Title = dto.Title,
            Nodes = dto.Nodes
                .OrderBy(n => n.Number)
                .Select(n => new LegalDocumentNodeFormViewModel
                {
                    Number = n.Number,
                    Title = n.Title,
                    ContentDelta = n.ContentDelta
                })
                .ToList()
        };

        ViewData["CurrentPage"] = GCommon.ApplicationRules.Pages.Admin.LegalDocuments;
        return View("EditDocument", model);
    }

    private async Task<EditLegalDocumentViewModel> HydrateEditModelAsync(EditLegalDocumentViewModel model)
    {
        ServiceResult<LegalDocumentDto> latestResult = await _adminLegalDocumentsService.GetLatestDocumentForEditReadonlyAsync(model.DocumentType);
        if (!latestResult.Success || !latestResult.Found || latestResult.Result is null)
        {
            model.CurrentVersion = Math.Max(1, model.CurrentVersion);
            model.Nodes ??= new();
            return model;
        }

        LegalDocumentDto latest = latestResult.Result;
        model.CurrentVersion = Math.Max(1, latest.Version);
        model.DocumentTypeDisplayName = model.DocumentType.GetDisplayName();

        if (model.Nodes is null || model.Nodes.Count == 0)
        {
            model.Nodes = latest.Nodes
                .OrderBy(n => n.Number)
                .Select(n => new LegalDocumentNodeFormViewModel
                {
                    Number = n.Number,
                    Title = n.Title,
                    ContentDelta = n.ContentDelta
                })
                .ToList();
        }

        return model;
    }
}
