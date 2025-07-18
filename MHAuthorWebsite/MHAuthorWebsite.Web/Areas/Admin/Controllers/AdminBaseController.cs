using MHAuthorWebsite.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

//[Authorize(Roles = "Admin")]
[Area("Admin")]
public class AdminBaseController : BaseController
{
}