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

        /*
         *  TODO: make proxy error logs saveable in file
         */
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

            // load balancer
            var destination = _loadBalancer.SelectDestination(resolvedRoute.Cluster, _healthTracker);
            if (destination is null) 
            { 
                context.Response.StatusCode = 503; 
                return; 
            }
            // forward request with resolved route
            var response = await _requestForwarder.ForwardAsync(context, resolvedRoute, destination, ct);

            // forward response
            await _responseForwarder.ForwardAsync(context, response, ct);
        }
    }
}
