using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Controllers;

[Authorize]
public class BaseController : Controller
{
    public bool IsUserAuthenticated() => User.Identity?.IsAuthenticated ?? false;

    public string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
}
