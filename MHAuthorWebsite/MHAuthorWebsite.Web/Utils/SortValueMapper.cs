using MHAuthorWebsite.Data.Models;
using System.Linq.Expressions;
using static MHAuthorWebsite.GCommon.ApplicationRules.SortTypes;

namespace MHAuthorWebsite.Web.Utils;

public static class SortValueMapper
{
    public static readonly Dictionary<string, (bool descending, Expression<Func<Product, object>>? expression)> SortMap =
        new()
        {
            [Recommended] = (true, null),
            [PriceDesc] = (true, p => p.Discounts
                .Where(d => d.EndDate > DateTime.Now)
                .OrderBy(d => d.NewPrice)
                .Select(d => (decimal?)d.NewPrice)
                .FirstOrDefault() ?? p.Price),
            [PriceAsc] = (false, p => p.Discounts
                .Where(d => d.EndDate > DateTime.Now)
                .OrderBy(d => d.NewPrice)
                .Select(d => (decimal?)d.NewPrice)
                .FirstOrDefault() ?? p.Price),
            [Likes] = (true, p => p.Likes.Count)
        };
}