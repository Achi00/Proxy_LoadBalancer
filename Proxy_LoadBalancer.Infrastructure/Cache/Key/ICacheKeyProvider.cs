using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Cache.Key
{
    public interface ICacheKeyProvider
    {
        string GenerateKey(HttpContext context, string[]? varyHeaders = null);
    }
}
