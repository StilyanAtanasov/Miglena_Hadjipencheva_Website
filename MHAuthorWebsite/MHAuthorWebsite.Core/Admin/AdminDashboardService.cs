using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Core.Dtos.Admin.Dashboard;
using MHAuthorWebsite.Core.Extensions;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using static MHAuthorWebsite.GCommon.ApplicationRules.DataCollection;

namespace MHAuthorWebsite.Core.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IApplicationRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(IApplicationRepository repository, UserManager<ApplicationUser> userManager, ILogger<AdminDashboardService> logger)
    {
        _repository = repository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetDashboardStatisticsAsync()
    {
        DateTime periodStart = DateTime.UtcNow.AddDays(-UsersActivityForPeriod);

        ApplicationUser[] users = await _userManager.Users.ToArrayAsync();
        List<ApplicationUser> admins = (List<ApplicationUser>)await _userManager.GetUsersInRoleAsync("Admin");

        var result = new AdminDashboardDto
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
                    SoldCount = pr.Orders
                        .Where(op => op.Order.Status == OrderStatus.Delivered)
                        .Sum(op => op.Quantity)
                })
                .OrderByDescending(pr => pr.SoldCount)
                .ThenByDescending(pr => pr.LikesCount)
                .Take(10)
                .ToList()
        };

        _logger.LogInformation("Successfully retrieved dashboard statistics.");
        return result;
    }

}