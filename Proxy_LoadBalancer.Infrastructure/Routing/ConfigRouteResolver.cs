using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Routing
{
    public class ConfigRouteResolver
    {
        private readonly ProxyOption _options;
        public ConfigRouteResolver(IOptions<ProxyOption> options)
        {
            _options = options.Value;
        }

        // TODO: "PathMatch": "Exact", "Prefix", "Regex" ...
        public ResolvedRoute? Resolve(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;

            // as default more specific route wins, or explicily defined Priority
            var route = _options.Routes.Where(r =>
                path.StartsWithSegments(r.Match.Path) &&
                (r.Match.Methods == null || r.Match.Methods.Contains(method)))
                .OrderBy(r => r.Match.Path.Length)
                .ThenBy(r => r.Priority)
                .FirstOrDefault();

            if (route == null)
            {
                return null;
            }

            if (!_options.Clusters.TryGetValue(route.ClusterId, out var cluster))
            {
                return null;
            }

            return new ResolvedRoute
            {
                Route = route,
                Cluster = cluster
            };
        }
    }
}
