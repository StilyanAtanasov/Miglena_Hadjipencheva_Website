using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Web.ViewModels.Contacts;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class ContactsController : BaseController
{
    private readonly IEmailService _emailService;

    public ContactsController(IEmailService emailService) => _emailService = emailService;


    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> SendEmail([FromBody] ContactFormViewModel model)
    {
        string body = $"Име: {model.Name}\nИмейл: {model.Email}\n\n{model.Message}";
        await _emailService.SendEmailAsync("notifications@miglena-hadjipencheva.com", model.Subject, body, false);
        return Ok();
    }
}
