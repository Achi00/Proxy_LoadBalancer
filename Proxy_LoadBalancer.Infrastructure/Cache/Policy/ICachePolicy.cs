using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Cache.Policy
{
    public interface ICachePolicy
    {
        TimeSpan? GetTtl(HttpResponse response);
        bool IsRequestCacheable(HttpContext context);
        bool IsResponseCacheable(HttpContext context);
        bool MustRevalidate(HttpContext context);
        string[] GetVaryHeaders(HttpResponse response);
    }
}