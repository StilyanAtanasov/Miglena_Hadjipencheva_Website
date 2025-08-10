using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MHAuthorWebsite.Web.Controllers;

public class HomeController : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }
}