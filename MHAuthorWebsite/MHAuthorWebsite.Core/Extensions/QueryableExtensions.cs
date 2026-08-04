namespace MHAuthorWebsite.Core.Extensions;

public static class QueryableExtensions
{
    public static Func<IQueryable, CancellationToken, Task<object>>? ToListProvider { get; set; }

    public static Func<IQueryable, CancellationToken, Task<object>>? ToArrayProvider { get; set; }

    public static Func<IQueryable, CancellationToken, Task<object?>>? FirstOrDefaultProvider { get; set; }

    public static Func<IQueryable, CancellationToken, Task<bool>>? AnyProvider { get; set; }

    public static Func<IQueryable, CancellationToken, Task<int>>? CountProvider { get; set; }

    public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> source, CancellationToken ct = default)
    {
        CheckProvider(ToListProvider);
        CheckAnonymous<T>();
        var result = await ToListProvider!(source, ct);
        return (List<T>)result;
    }

    public static async Task<T[]> ToArrayAsync<T>(this IQueryable<T> source, CancellationToken ct = default)
    {
        CheckProvider(ToArrayProvider);
        CheckAnonymous<T>();
        var result = await ToArrayProvider!(source, ct);
        return (T[])result;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> source, CancellationToken ct = default)
    {
        CheckProvider(FirstOrDefaultProvider);
        CheckAnonymous<T>();
        var result = await FirstOrDefaultProvider!(source, ct);
        return (T?)result;
    }

    public static async Task<bool> AnyAsync<T>(this IQueryable<T> source, CancellationToken ct = default)
    {
        CheckProvider(AnyProvider);
        return await AnyProvider!(source, ct);
    }

    public static async Task<int> CountAsync<T>(this IQueryable<T> source, CancellationToken ct = default)
    {
        CheckProvider(CountProvider);
        return await CountProvider!(source, ct);
    }

    private static void CheckProvider(object? provider)
    {
        if (provider == null)
            throw new InvalidOperationException("QueryBridge not initialized. Call QueryBridge.Initialize() in the Data/Infrastructure project.");
    }

    private static void CheckAnonymous<T>()
    {
        Type type = typeof(T);
        bool isAnonymous =
            Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
            && type.IsGenericType && type.Name.Contains("AnonymousType");

        if (isAnonymous)
        {
            throw new InvalidOperationException(
                $"Bridge Error: Anonymous types cannot be used in the Core layer. " +
                $"Please create a formal class/DTO for the selection in method: {type.FullName}");
        }
    }
}