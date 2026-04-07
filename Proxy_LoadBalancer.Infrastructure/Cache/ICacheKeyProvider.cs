using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Cache
{
    public interface ICacheKeyProvider
    {
        string GenerateKey(HttpContext context, string[] varyHeaders);
    }
}
