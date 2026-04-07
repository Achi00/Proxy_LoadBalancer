using Proxy_LoadBalancer.Infrastructure.Models;

namespace Proxy_LoadBalancer.Infrastructure.Cache
{
    public interface IResponseCacheStore
    {
        bool TryGet(string key, out CachedResponse? response);
        void Set(string key, CachedResponse response, TimeSpan ttl);
        void Remove(string key);
    }

}
