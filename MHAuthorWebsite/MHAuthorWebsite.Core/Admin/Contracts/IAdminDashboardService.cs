using MHAuthorWebsite.Core.Dtos.Admin.Dashboard;

namespace MHAuthorWebsite.Core.Admin.Contracts;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardStatisticsAsync();
}