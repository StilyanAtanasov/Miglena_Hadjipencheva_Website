using MHAuthorWebsite.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MHAuthorWebsite.Data.Shared;

public static class QueryBridge
{
    public static void Initialize()
    {
        // We map the Core static properties to the real EF Core extension methods
        QueryableExtensions.ToListProvider = async (query, ct) =>
            await EntityFrameworkQueryableExtensions.ToListAsync((dynamic)query, ct);

        QueryableExtensions.ToArrayProvider = async (query, ct) =>
            await EntityFrameworkQueryableExtensions.ToArrayAsync((dynamic)query, ct);

        QueryableExtensions.FirstOrDefaultProvider = async (query, ct) =>
            await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync((dynamic)query, ct);

        QueryableExtensions.AnyProvider = async (query, ct) =>
            await EntityFrameworkQueryableExtensions.AnyAsync((dynamic)query, ct);

        QueryableExtensions.CountProvider = async (query, ct) =>
            await EntityFrameworkQueryableExtensions.CountAsync((dynamic)query, ct);
    }
}