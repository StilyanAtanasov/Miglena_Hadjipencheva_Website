using MHAuthorWebsite.Core.Common.Utils;
using MHAuthorWebsite.Web.ViewModels.Admin.UserManagement;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminUserManagementService
{
    Task<ICollection<UserSummaryRowViewModel>> GetAllUsersReadonlyAsync();

    Task<ServiceResult> AssignRoleToUserAsync(string userId, string roleName);
}