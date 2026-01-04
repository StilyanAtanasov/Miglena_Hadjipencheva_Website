using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Dtos.Admin.UserManagement;
using MHAuthorWebsite.Web.Utils.Attributes;
using MHAuthorWebsite.Web.Utils.Enums;
using MHAuthorWebsite.Web.ViewModels.Admin.UserManagement;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminUserManagementController : AdminBaseController
{
    private readonly IAdminUserManagementService _adminUserManagementService;

    public AdminUserManagementController(IAdminUserManagementService adminUserManagementService)
        => _adminUserManagementService = adminUserManagementService;

    [SecurityHeaders(CspFeature.Notifications)]
    [HttpGet]
    public async Task<IActionResult> ManageUsers()
    {
        ICollection<UserSummaryRowDto> dto = await _adminUserManagementService.GetAllUsersReadonlyAsync();
        ICollection<UserSummaryRowViewModel> vm = dto
            .Select(u => new UserSummaryRowViewModel()
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                IsBanned = u.IsBanned,
                IsDeleted = u.IsDeleted,
                IsActive = u.IsActive,
                IsAdmin = u.IsAdmin,
            })
            .ToArray();

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        ServiceResult result = await _adminUserManagementService.AssignRoleToUserAsync(userId, roleName);
        if (!result.Success) return StatusCode(500);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ToggleIsBanned(string userId)
    {
        ServiceResult<bool> result = await _adminUserManagementService.ToggleIsBannedStatusAsync(userId);
        if (!result.Found) return BadRequest();
        if (!result.Success)
        {
            if (result.Errors.ContainsKey("AlreadyDeleted") || result.Errors.ContainsKey("IsAdmin"))
                return BadRequest();

            return StatusCode(500);
        }

        return Ok(result.Result);
    }

    [HttpGet]
    public async Task<IActionResult> UserDetails(string userId)
    {
        ServiceResult<UserDetailsDto> result = await _adminUserManagementService.GetUserDetailsReadonlyAsync(userId);
        if (!result.Found) return BadRequest();

        UserDetailsDto dto = result.Result!;
        UserDetailsViewModel viewModel = new()
        {
            Id = dto.Id,
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            IsBanned = dto.IsBanned,
            IsDeleted = dto.IsDeleted,
            LastActive = dto.LastActive,
            DateJoined = dto.DateJoined,
            IsActive = dto.IsActive,
            IsAdmin = dto.IsAdmin,
        };

        return PartialView("_UserDetails", viewModel);
    }
}