using MHAuthorWebsite.Data.Models;
using MHAuthorWebsite.Data.Shared.Filters.Contracts;
using MHAuthorWebsite.Data.Shared.Filters.Criteria;

namespace MHAuthorWebsite.Data.Shared.Filters;

public class AllOrdersFilter : IFilter<Order>
{
    private readonly AllOrdersFilterCriteria _criteria;

    public AllOrdersFilter(AllOrdersFilterCriteria criteria) => _criteria = criteria;

    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        if (!string.IsNullOrWhiteSpace(_criteria.Status))
            if (Enum.TryParse(typeof(Models.Enums.OrderStatus), _criteria.Status, true, out object? status))
                query = query.Where(o => o.Status == (Models.Enums.OrderStatus)status);

        return query;
    }
}