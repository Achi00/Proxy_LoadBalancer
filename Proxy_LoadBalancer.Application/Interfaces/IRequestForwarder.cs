using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Application.Interfaces
{
    public interface IRequestForwarder
    {
        Task<HttpResponseMessage> ForwardAsync(HttpContext context, string targetUrl, CancellationToken ct);
    }
}
