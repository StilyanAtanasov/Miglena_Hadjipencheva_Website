namespace MHAuthorWebsite.Core.Contracts;

public interface IFastCacheService : ICacheService
{
    void SetFireAndForget<T>(string key, T value, TimeSpan ttl);
}