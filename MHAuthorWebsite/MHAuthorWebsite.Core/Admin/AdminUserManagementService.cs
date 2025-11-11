using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.DataCollection;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core.Admin;

public class AdminUserManagementService : IAdminUserManagementService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUserManagementService(IApplicationRepository repository, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _repository = repository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ICollection<UserSummaryRowViewModel>> GetAllUsersReadonlyAsync()
    {
        ICollection<ApplicationUser> adminUsers = await _userManager.GetUsersInRoleAsync(AdminRoleName);

        return await _repository
            .AllReadonly<ApplicationUser>()
            .Select(u => new UserSummaryRowViewModel
            {
                Id = u.Id,
                Name = !u.IsDeleted ? u.Name : "",
                Email = !u.IsDeleted ? u.Email : "",
                IsActive = u.LastActive > DateTime.Now.AddDays(-UsersActivityForPeriod),
                IsAdmin = adminUsers.Contains(u),
                IsDeleted = u.IsDeleted
            })
            .ToArrayAsync();
    }

    public async Task<ServiceResult> AssignRoleToUserAsync(string userId, string roleName)
    {
        bool roleExists = _roleManager.Roles.Any(r => r.Name == roleName);
        if (!roleExists) return ServiceResult.Failure(new() { ["Role"] = $"Role with name '{roleName}' does not exist." });

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ServiceResult.Failure(new() { ["User"] = $"User with ID '{userId}' does not exist." });

        IdentityResult result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded) return ServiceResult.Failure(new()
        {
            ["RoleAssignment"] =
            $"Failed to assign role '{roleName}' to user '{userId}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
        });

        return ServiceResult.Ok();
    }
}