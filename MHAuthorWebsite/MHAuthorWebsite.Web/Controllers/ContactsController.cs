using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Contacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class ContactsController : BaseController
{
    private readonly IContactsService _contactsService;

    public ContactsController(IContactsService contactsService) => _contactsService = contactsService;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SendEmail([FromBody] ContactFormViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

        ServiceResult sr = await _contactsService.SendContactMessageAsync(model, GetUserId());
        if (!sr.HasPermission) return Forbid();

        return Ok();
    }
}
