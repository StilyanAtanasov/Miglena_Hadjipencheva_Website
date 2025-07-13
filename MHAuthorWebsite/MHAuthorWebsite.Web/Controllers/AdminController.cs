using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class AdminController : BaseController
{
    public IActionResult Dashboard()
    {
        return View();
    }
}