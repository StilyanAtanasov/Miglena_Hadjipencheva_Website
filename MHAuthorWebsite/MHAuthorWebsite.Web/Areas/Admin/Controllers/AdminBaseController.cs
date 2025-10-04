using MHAuthorWebsite.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

[Authorize(Roles = AdminRoleName)]
[Area(AdminRoleName)]
[Route($"{AdminRoleName}/[controller]/[action]")]
public class AdminBaseController : BaseController
{
}