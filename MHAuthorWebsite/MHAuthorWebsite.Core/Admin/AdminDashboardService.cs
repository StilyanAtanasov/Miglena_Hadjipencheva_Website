using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MHAuthorWebsite.GCommon.ApplicationRules.DataCollection;

namespace MHAuthorWebsite.Core.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDashboardService(IApplicationRepository repository, UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetDashboardStatisticsAsync()
    {
        DateTime periodStart = DateTime.UtcNow.AddDays(-UsersActivityForPeriod);

        ApplicationUser[] users = await _userManager.Users.ToArrayAsync();
        List<ApplicationUser> admins = (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync("Admin");

        return new AdminDashboardViewModel
        {
            UsersCount = users.Length - admins.Count,
            NewUsersCount = users.Count(u => u.RegisteredOn >= periodStart) - admins.Count(u => u.RegisteredOn >= periodStart),
            ActiveUsersCount = users.Count(u => u.LastActive >= periodStart) - admins.Count(u => u.LastActive >= periodStart),
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

}