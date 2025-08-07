using MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardStatisticsAsync();
}