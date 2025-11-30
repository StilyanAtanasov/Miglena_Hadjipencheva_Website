using StackExchange.Redis;

namespace MHAuthorWebsite.Core.Contracts;

public interface ICacheService
{
    IBatch CreateBatch();

    Task<T?> GetAsync<T>(string key);

    Task SetAsync<T>(string key, T value, TimeSpan ttl);

    Task RemoveAsync(string key);
}