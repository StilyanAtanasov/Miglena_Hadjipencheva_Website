using MHAuthorWebsite.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

//[Authorize(Roles = "Admin")]
[Area("Admin")]
[Route("Admin/[controller]/[action]")]
public class AdminBaseController : BaseController
{
}