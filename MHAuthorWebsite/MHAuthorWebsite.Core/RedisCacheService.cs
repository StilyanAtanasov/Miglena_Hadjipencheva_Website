using MHAuthorWebsite.Core.Contracts;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MHAuthorWebsite.Core;

public class RedisCacheService : IFastCacheService
{
    private readonly IDatabase _db;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public RedisCacheService(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public IBatch CreateBatch() => _db.CreateBatch();

    public async Task<T?> GetAsync<T>(string key)
    {
        RedisValue value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>(value!, JsonOptions) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
    {
        string json = JsonSerializer.Serialize(value, JsonOptions);
        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task RemoveAsync(string key) => await _db.KeyDeleteAsync(key);

    public void SetFireAndForget<T>(string key, T value, TimeSpan ttl)
    {
        string json = JsonSerializer.Serialize(value, JsonOptions);
        _db.StringSet(key, json, ttl, flags: CommandFlags.FireAndForget);
    }
}