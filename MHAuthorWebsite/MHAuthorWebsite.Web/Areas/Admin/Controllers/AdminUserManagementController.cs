using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminUserManagementController : AdminBaseController
{
    private readonly IAdminUserManagementService _adminUserManagementService;

    public AdminUserManagementController(IAdminUserManagementService adminUserManagementService)
        => _adminUserManagementService = adminUserManagementService;

    [HttpGet]
    public async Task<IActionResult> ManageUsers()
     => View(await _adminUserManagementService.GetAllUsersReadonlyAsync());

    [HttpPost]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        ServiceResult result = await _adminUserManagementService.AssignRoleToUserAsync(userId, roleName);
        if (!result.Success) return StatusCode(500);

        return Ok();
    }
}