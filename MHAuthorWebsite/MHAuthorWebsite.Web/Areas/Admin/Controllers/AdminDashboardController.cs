using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminDashboardController : AdminBaseController
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService) => _adminDashboardService = adminDashboardService;

    public async Task<IActionResult> Dashboard()
    {
        AdminDashboardViewModel model = await _adminDashboardService.GetDashboardStatisticsAsync();
        return View(model);
    }
}