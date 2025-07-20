using MHAuthorWebsite.Data.Models;
using System.Linq.Expressions;

namespace MHAuthorWebsite.Web.Utils;

public static class SortValueMapper
{
    public static readonly Dictionary<string, (bool descending, Expression<Func<Product, object>>? expression)> SortMap =
        new()
        {
            ["recommended"] = (true, null),
            ["price_desc"] = (true, p => p.Price),
            ["price_asc"] = (false, p => p.Price),
            ["likes"] = (true, p => p.Likes.Count)
        };
}