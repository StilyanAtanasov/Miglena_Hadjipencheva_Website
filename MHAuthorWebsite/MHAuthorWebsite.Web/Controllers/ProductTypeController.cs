using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

public class ProductTypeController : BaseController
{
    [HttpGet]
    public IActionResult AddProductType()
    {
        return View();
    }
}
