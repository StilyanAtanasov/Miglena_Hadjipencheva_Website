using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
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
        ICollection<ContactRequestCardViewModel> contactRequests =
            await _adminContactRequestsService.GetContactRequestsPagedReadonlyAsync(page);

        return View(contactRequests);
    }

    [HttpGet]
    public async Task<IActionResult> ContactRequestDetails(Guid requestId)
    {
        ServiceResult<ContactRequestDetailsViewModel> sr =
            await _adminContactRequestsService.GetContactRequestDetailsReadonlyAsync(requestId);
        if (!sr.Found) return NotFound();

        return View(sr.Result);
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