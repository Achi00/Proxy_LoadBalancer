using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Routing
{
    public class ConfigRouteResolver
    {
        private readonly ProxyOption _options;
        private readonly List<CompiledRoute> _compiledRoutes;

        public ConfigRouteResolver(IOptions<ProxyOption> options)
        {
            _options = options.Value;
            _compiledRoutes = _options.Routes
                .Select(r => new CompiledRoute(r, _options.Defaults))
                // pre-sort by base score
                .OrderByDescending(r => r.BaseScore)
                .ToList();
        }

        public ResolvedRoute? Resolve(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;

            RouteOption? bestRoute = null;
            int bestScore = 0;

            foreach (var compiled in _compiledRoutes)
            {
                // early exit: pre-sorted by BaseScore, if current base <= bestScore
                // nothing after can possibly beat it, list already sorder so no point to check further
                if (compiled.BaseScore <= bestScore)
                {
                    break;
                }

                var score = compiled.GetScore(path, method);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRoute = compiled.Route;
                }
            }

            if (bestRoute is null) return null;

            if (!_options.Clusters.TryGetValue(bestRoute.ClusterId, out var cluster))
            {
                return null;
            }

            return new ResolvedRoute { Route = bestRoute, Cluster = cluster };
        }
    }
}
