using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.ContactRequests;
using MHAuthorWebsite.Web.ViewModels.Admin.ContactRequests;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminContactRequestsController : AdminBaseController
{
    private readonly IAdminContactRequestsService _adminContactRequestsService;

    public AdminContactRequestsController(IAdminContactRequestsService adminContactRequestsService)
        => _adminContactRequestsService = adminContactRequestsService;

    [HttpGet]
    public async Task<IActionResult> ContactRequestsBoard(int page = 1)
    {
        ICollection<ContactRequestCardDto> contactRequests =
            await _adminContactRequestsService.GetContactRequestsPagedReadonlyAsync(page);

        ICollection<ContactRequestCardViewModel> contactRequestViewModels =
            contactRequests.Select(req => new ContactRequestCardViewModel
            {
                Id = req.Id,
                Subject = req.Subject,
                MessagePreview = req.MessagePreview,
                Name = req.Name,
                CreatedOn = req.CreatedOn,
                Email = req.Email,
                IsAnswered = req.IsAnswered,
            }).ToList();

        return View(contactRequestViewModels);
    }

    [HttpGet]
    public async Task<IActionResult> ContactRequestDetails(Guid requestId)
    {
        ServiceResult<ContactRequestDetailsDto> sr =
            await _adminContactRequestsService.GetContactRequestDetailsReadonlyAsync(requestId);
        if (!sr.Found) return NotFound();

        ContactRequestDetailsDto dto = sr.Result!;

        ContactRequestDetailsViewModel viewModel = new()
        {
            Id = dto.Id,
            Subject = dto.Subject,
            Message = dto.Message,
            Name = dto.Name,
            Email = dto.Email,
            CreatedOn = dto.CreatedOn,
            IsAnswered = dto.IsAnswered,
            ReplyMessage = dto.ReplyMessage,
            RepliedOn = dto.RepliedOn
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ReplyToRequest([FromForm] Guid requestId, [FromForm] string replyMessage)
    {
        if (!ModelState.IsValid) return BadRequest();

        ServiceResult sr = await _adminContactRequestsService
            .ReplyToContactRequestAsync(requestId, replyMessage, GetUserId()!);
        if (!sr.Found) return BadRequest();

        return RedirectToAction(nameof(ContactRequestsBoard));
    }
}