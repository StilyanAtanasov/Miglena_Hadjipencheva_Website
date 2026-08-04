using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin.Dashboard;
using MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace MHAuthorWebsite.Web.Areas.Admin.Controllers;

public class AdminDashboardController : AdminBaseController
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService) => _adminDashboardService = adminDashboardService;

    public async Task<IActionResult> Dashboard()
    {
        AdminDashboardDto model = await _adminDashboardService.GetDashboardStatisticsAsync();

        AdminDashboardViewModel viewModel = new()
        {
            ActiveUsersCount = model.ActiveUsersCount,
            NewUsersCount = model.NewUsersCount,
            UsersCount = model.UsersCount,
            ProductsList = model.ProductsList
                .Select(p => new AdminDashboardProductsViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SoldCount = p.SoldCount,
                    LikesCount = p.LikesCount,
                })
                .ToList(),
        };

        return View(viewModel);
    }
}