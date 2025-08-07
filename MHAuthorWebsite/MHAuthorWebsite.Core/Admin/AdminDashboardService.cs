using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Core.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminDashboardService(IApplicationRepository repository, UserManager<IdentityUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetDashboardStatisticsAsync()
    => new()
    {
        UsersCount = _userManager.Users.Count() - (await _userManager.GetUsersInRoleAsync("Admin")).Count,
        ProductsList = _repository
                .AllReadonly<Product>()
                .Include(p => p.Likes)
                .Select(pr => new AdminDashboardProductsViewModel
                {
                    Name = pr.Name,
                    LikesCount = pr.Likes.Count
                })
                .OrderByDescending(pr => pr.LikesCount)
                .ToList()
    };
}