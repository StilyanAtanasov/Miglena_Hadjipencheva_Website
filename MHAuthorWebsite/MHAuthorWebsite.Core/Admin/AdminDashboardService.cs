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
                .Include(p => p.Orders)
                .Select(pr => new AdminDashboardProductsViewModel
                {
                    Id = pr.Id,
                    Name = pr.Name,
                    LikesCount = pr.Likes.Count,
                    SoldCount = pr.Orders.Sum(op => op.Quantity) // TODO SELECT accepted orders only
                })
                .OrderByDescending(pr => pr.SoldCount)
                .ThenByDescending(pr => pr.LikesCount)
                .Take(10)
                .ToList()
        };
}