using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminController : AdminBaseController
{
    public IActionResult Dashboard()
    {
        return View();
    }
}