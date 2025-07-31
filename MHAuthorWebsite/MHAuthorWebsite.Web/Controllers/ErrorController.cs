using MHAuthorWebsite.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MHAuthorWebsite.Web.Controllers;

public class ErrorController : BaseController
{
    [AllowAnonymous]
    [Route("Error/Error/{statusCode}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode) =>
        statusCode switch
        {
            400 => View("400"),
            401 => View("401"),
            403 => View("403"),
            404 => View("404"),
            _ => View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })
        };

    /* TODO Send notification to admin */
}