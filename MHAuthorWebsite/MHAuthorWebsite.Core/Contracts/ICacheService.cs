namespace MHAuthorWebsite.Core.Contracts;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);

    Task<IEnumerable<T?>> GetBatchAsync<T>(IEnumerable<string> keys);

    Task SetAsync<T>(string key, T value, TimeSpan ttl);

    Task SetBatchAsync<T>(IDictionary<string, T> values, TimeSpan ttl, bool fireAndForget = false);

    Task RemoveAsync(string key);
}