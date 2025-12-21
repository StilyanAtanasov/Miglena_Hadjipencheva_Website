using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Core.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin.UserManagement;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
using static MHAuthorWebsite.GCommon.ApplicationRules.CacheKeys;
using static MHAuthorWebsite.GCommon.ApplicationRules.DataCollection;
using static MHAuthorWebsite.GCommon.ApplicationRules.Roles;

namespace MHAuthorWebsite.Core.Admin;

public class AdminUserManagementService : IAdminUserManagementService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IFastCacheService _cache;

    public AdminUserManagementService(IFastCacheService cache, IApplicationRepository repository, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _cache = cache;
        _repository = repository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ICollection<UserSummaryRowDto>> GetAllUsersReadonlyAsync()
    {
        ICollection<ApplicationUser> adminUsers = await _userManager.GetUsersInRoleAsync(AdminRoleName);

        return await _repository
            .AllReadonly<ApplicationUser>()
            .Select(u => new UserSummaryRowDto
            {
                Id = u.Id,
                Name = !u.IsDeleted ? u.Name : "",
                Email = !u.IsDeleted ? u.Email : "",
                IsActive = u.LastActive > DateTime.Now.AddDays(-UsersActivityForPeriod),
                IsAdmin = adminUsers.Contains(u),
                IsBanned = u.IsBanned,
                IsDeleted = u.IsDeleted
            })
            .ToArrayAsync();
    }

    public async Task<ServiceResult<UserDetailsDto>> GetUserDetailsReadonlyAsync(string userId)
    {
        ApplicationUser? user = await _repository
            .WhereReadonly<ApplicationUser>(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if (user == null) return ServiceResult<UserDetailsDto>.NotFound();

        UserDetailsDto userDetails = new()
        {
            Id = user.Id,
            Name = !user.IsDeleted ? user.Name : "",
            Email = !user.IsDeleted ? user.Email : "",
            Phone = !user.IsDeleted ? user.PhoneNumber : "",
            IsActive = user.LastActive > DateTime.Now.AddDays(-UsersActivityForPeriod),
            IsAdmin = _userManager.IsInRoleAsync(user, AdminRoleName).Result,
            IsDeleted = user.IsDeleted,
            IsBanned = user.IsBanned,
            DateJoined = user.RegisteredOn,
            LastActive = user.LastActive
        };

        return ServiceResult<UserDetailsDto>.Ok(userDetails);
    }

    public async Task<ServiceResult> AssignRoleToUserAsync(string userId, string roleName)
    {
        bool roleExists = _roleManager.Roles.Any(r => r.Name == roleName);
        if (!roleExists) return ServiceResult.Failure(new() { ["Role"] = $"Role with name '{roleName}' does not exist." });

        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ServiceResult.Failure(new() { ["User"] = $"User with ID '{userId}' does not exist." });
        if (user.IsDeleted) return ServiceResult.Failure(new() { ["AlreadyDeleted"] = "Cannot assign role to a deleted user." });

        user.IsBanned = false; // Unban user when assigning any role
        await _userManager.UpdateAsync(user);

        IdentityResult result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded) return ServiceResult.Failure(new()
        {
            ["RoleAssignment"] =
            $"Failed to assign role '{roleName}' to user '{userId}': {string.Join(", ", result.Errors.Select(e => e.Description))}"
        });

        if (roleName == AdminRoleName) await _cache.RemoveAsync(AdminIdsKey());

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<bool>> ToggleIsBannedStatusAsync(string userId)
    {
        ApplicationUser? user = _userManager.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return ServiceResult<bool>.NotFound();
        if (user.IsDeleted) return ServiceResult<bool>.Failure(new() { ["AlreadyDeleted"] = "Cannot change banned status of a deleted user." });
        if (await _userManager.IsInRoleAsync(user, AdminRoleName)) return ServiceResult<bool>.Failure(new() { ["IsAdmin"] = "Cannot change banned status of an admin user." });

        user.IsBanned = !user.IsBanned;
        await _userManager.UpdateAsync(user);

        return ServiceResult<bool>.Ok(user.IsBanned);
    }
}