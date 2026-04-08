using Microsoft.Extensions.Caching.Memory;
using Proxy_LoadBalancer.Infrastructure.Models;

namespace Proxy_LoadBalancer.Infrastructure.Cache
{
    public class MemoryResponseCacheStore : IResponseCacheStore
    {
        private readonly IMemoryCache _cache;

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }
        public bool TryGet(string key, out CachedResponse? response)
        {
            return _cache.TryGetValue(key, out response);
        }

        public void Set(string key, CachedResponse response, TimeSpan ttl)
        {
            _cache.Set(key, response, ttl);
        }
    }
}
