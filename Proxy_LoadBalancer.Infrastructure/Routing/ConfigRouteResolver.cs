using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Application.DTOs;
using Proxy_LoadBalancer.Application.Interfaces;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Routing
{
    internal class ConfigRouteResolver
    {
        private readonly ProxyOption _options;
        public ConfigRouteResolver(IOptions<ProxyOption> options)
        {
            _options = options.Value;
        }
        public RouteOption? Resolve(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;

            return _options.Routes.FirstOrDefault(r =>
                path.StartsWithSegments(r.Match.Path) &&
                (r.Match.Methods == null || r.Match.Methods.Contains(method)));
        }
    }
}
