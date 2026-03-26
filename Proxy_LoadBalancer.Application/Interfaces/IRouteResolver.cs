using Microsoft.AspNetCore.Http;
using Proxy_LoadBalancer.Application.DTOs;

namespace Proxy_LoadBalancer.Application.Interfaces
{
    public interface IRouteResolver
    {
        ResolvedRouteResponse? Resolve(HttpRequest request);
    }
}
