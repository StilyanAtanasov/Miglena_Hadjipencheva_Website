using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MHAuthorWebsite.Web.Areas.Identity.Pages.Account.Manage;

public class EmailModel : PageModel
{
    public IActionResult OnGet() => StatusCode(404);

    public IActionResult OnPost() => StatusCode(404);
}