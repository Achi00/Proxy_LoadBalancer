using Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders;
using Proxy_LoadBalancer.Infrastructure.Health;
using Proxy_LoadBalancer.Infrastructure.LoadBalancer;
using Proxy_LoadBalancer.Infrastructure.Routing;

namespace Proxy_LoadBalancer.Host.Middleware
{
    public class ProxyMiddleware : IMiddleware
    {
        private readonly ConfigRouteResolver _routeResolver;
        private readonly HttpRequestForwarder _requestForwarder;
        private readonly HttpResponseForwarder _responseForwarder;
        private readonly ILoadBalancer _loadBalancer;
        private readonly PassiveHealthTracker _healthTracker;

        public ProxyMiddleware(ConfigRouteResolver routeResolver, HttpRequestForwarder requestForwarder, HttpResponseForwarder responseForwarder, ILoadBalancer loadBalancer, PassiveHealthTracker healthTracker)
        {
            _routeResolver = routeResolver;
            _responseForwarder = responseForwarder;
            _requestForwarder = requestForwarder;
            _loadBalancer = loadBalancer;
            _healthTracker = healthTracker;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // tied to client connection
            var ct = context.RequestAborted;
            
            // resolve route
            var resolvedRoute = _routeResolver.Resolve(context);

            if (resolvedRoute == null)
            {
                // no route matched
                context.Response.StatusCode = 404;
                return;
            }

            // load balancer, if any destination port is down we will distribute it to another healthy destination
            var attempts = resolvedRoute.Cluster.Destinations.Count;

            for (int i = 0; i < attempts; i++)
            {
                var destination = _loadBalancer.SelectDestination(resolvedRoute.Cluster, _healthTracker);
                if (destination is null) { context.Response.StatusCode = 503; return; }

                try
                {
                    var response = await _requestForwarder.ForwardAsync(context, resolvedRoute, destination, ct);
                    await _responseForwarder.ForwardAsync(context, response, ct);

                    return;
                }
                catch (HttpRequestException)
                {
                    // already marked unhealthy in forwarder
                    // if no more destinations will mark as error 502
                    var hasHealthy = resolvedRoute.Cluster.Destinations
                        .Any(d => _healthTracker.IsHealthy(d.Address));

                    // in case no healthy route found
                    if (!hasHealthy)
                    {
                        break;
                    }

                    // only retry idempotent methods, for safety avoid POST, PUT and ect requests
                    if (!IsIdempotent(context.Request.Method))
                    {
                        break;
                    }
                }
            }

            context.Response.StatusCode = 502;
            //// forward request with resolved route
            //var response = await _requestForwarder.ForwardAsync(context, resolvedRoute, destination, ct);

            //// forward response
            //await _responseForwarder.ForwardAsync(context, response, ct);
        }

        private static bool IsIdempotent(string method) =>
            method.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
            method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) ||
            method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase);
    }
}
