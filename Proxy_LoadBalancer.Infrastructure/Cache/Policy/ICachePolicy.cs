using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Cache.Policy
{
    public interface ICachePolicy
    {
        TimeSpan? GetTtl(HttpResponseMessage response);
        bool IsRequestCacheable(HttpContext context);
        bool IsResponseCacheable(HttpResponseMessage response);
        bool MustRevalidate(HttpContext context);
        string[] GetVaryHeaders(HttpResponseMessage response);
    }
}