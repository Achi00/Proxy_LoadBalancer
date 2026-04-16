using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.RateLimiter.Key
{
    public interface IRateLimitKeyProvider
    {
        string GenerateKey(HttpContext context);
    }
}
