using MHAuthorWebsite.Core.Admin.Contracts;
using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared;
using MHAuthorWebsite.Web.ViewModels.Admin.Order;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Core.Admin;

public class AdminOrderService : IAdminOrderService
{
    private readonly IApplicationRepository _repository;

    public AdminOrderService(IApplicationRepository repository) => _repository = repository;

    public async Task<ICollection<AllOrdersListItemViewModel>> GetAllOrders()
        => await _repository
            .AllReadonly<Order>()
            .Include(o => o.OrderedProducts)
            .Include(o => o.User)
            .Include(o => o.Shipment)
            .Select(o => new AllOrdersListItemViewModel
            {
                Id = o.Id,
                OrderNumber = o.Shipment.OrderNumber,
                CustomerName = o.User.UserName!, // TODO Check if user has their data deleted
                OrderDate = o.Date,
                TotalAmount = o.OrderedProducts.Sum(op => op.UnitPrice * op.Quantity),
                IsAccepted = o.IsAccepted
            })
            .ToArrayAsync();
}