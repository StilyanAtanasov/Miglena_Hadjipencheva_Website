using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductTypeController : Controller
{
    [HttpGet]
    public IActionResult AddProductType()
    {
        return View();
    }
}
