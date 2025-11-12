using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Admin.UserManagement;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminUserManagementService
{
    Task<ICollection<UserSummaryRowViewModel>> GetAllUsersReadonlyAsync();

    Task<ServiceResult<UserDetailsViewModel>> GetUserDetailsReadonlyAsync(string userId);

    Task<ServiceResult> AssignRoleToUserAsync(string userId, string roleName);

    /// <summary>
    /// Toggles the banned status of the specified user.
    /// </summary>
    /// <remarks>This method asynchronously toggles the banned status of the user identified by <paramref
    /// name="userId"/>. The operation's success or failure is encapsulated in the returned <see
    /// cref="ServiceResult{T}"/>.</remarks>
    /// <param name="userId">The unique identifier of the user whose banned status is to be toggled. Cannot be null or empty.</param>
    /// <returns>A <see cref="ServiceResult{T}"/> containing a boolean value indicating the updated banned status of the user.
    /// The value is <see langword="true"/> if the user is now banned; otherwise, <see langword="false"/>.</returns>
    Task<ServiceResult<bool>> ToggleIsBannedStatusAsync(string userId);
}