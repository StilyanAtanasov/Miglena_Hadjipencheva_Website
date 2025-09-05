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
                    .ThenInclude(o => o.OrderedProducts)
                        .ThenInclude(op => op.Product)
                .Select(pr => new AdminDashboardProductsViewModel
                {
                    Id = pr.Id,
                    Name = pr.Name,
                    LikesCount = pr.Likes.Count,
                    SoldCount = pr.Orders
                        .Count(o => o.OrderedProducts.Any(p => p.Product.Id == pr.Id)) // TODO FIX
                })
                .OrderByDescending(pr => pr.SoldCount)
                .ThenByDescending(pr => pr.LikesCount)
                .Take(10)
                .ToList()
        };
}