using MHAuthorWebsite.Core.Filters.Criteria;
using MHAuthorWebsite.Core.Models;
using MHAuthorWebsite.Core.Models.Contracts;
using MHAuthorWebsite.Core.Models.Enums;

namespace MHAuthorWebsite.Core.Filters;

public class AllOrdersFilter : IFilter<Order>
{
    private readonly AllOrdersFilterCriteria _criteria;

    public AllOrdersFilter(AllOrdersFilterCriteria criteria) => _criteria = criteria;

    public IQueryable<Order> Apply(IQueryable<Order> query)
    {
        if (!string.IsNullOrWhiteSpace(_criteria.Status))
            if (Enum.TryParse(typeof(OrderStatus), _criteria.Status, true, out object? status))
                query = query.Where(o => o.Status == (OrderStatus)status);

        return query;
    }
}