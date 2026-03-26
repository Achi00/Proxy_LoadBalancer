using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Application.Interfaces
{
    public interface IRequestForwarder
    {
        Task ForwardAsync(HttpContext context, string targetUrl, CancellationToken ct);
    }
}
