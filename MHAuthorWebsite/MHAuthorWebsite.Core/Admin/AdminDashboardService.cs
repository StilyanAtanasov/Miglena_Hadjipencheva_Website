using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Contracts.DataServices;
using MHAuthorWebsite.Core.Dtos.Admin.Dashboard;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using Microsoft.AspNetCore.Identity;
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

    public async Task<AdminDashboardDto> GetDashboardStatisticsAsync()
    {
        DateTime periodStart = DateTime.UtcNow.AddDays(-UsersActivityForPeriod);

        ApplicationUser[] users = await _userManager.Users.ToArrayAsync();
        List<ApplicationUser> admins = (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync("Admin");

        return new AdminDashboardDto
        {
            UsersCount = users.Length - admins.Count,
            NewUsersCount = users.Count(u => u.RegisteredOn >= periodStart) - admins.Count(u => u.RegisteredOn >= periodStart),
            ActiveUsersCount = users.Count(u => u.LastActive >= periodStart) - admins.Count(u => u.LastActive >= periodStart),
            ProductsList = _repository
                .AllReadonly<Product>()
                .Select(pr => new AdminDashboardProductsDto
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