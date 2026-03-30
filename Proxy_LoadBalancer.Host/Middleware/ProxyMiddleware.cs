using Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders;
using Proxy_LoadBalancer.Infrastructure.Routing;

namespace Proxy_LoadBalancer.Host.Middleware
{
    public class ProxyMiddleware
    {
        private readonly ConfigRouteResolver _routeResolver;
        private readonly HttpRequestForwarder _requestForwarder;
        private readonly HttpResponseForwarder _responseForwarder;

        public ProxyMiddleware(ConfigRouteResolver routeResolver, HttpRequestForwarder requestForwarder, HttpResponseForwarder responseForwarder)
        {
            _routeResolver = routeResolver;
            _responseForwarder = responseForwarder;
            _requestForwarder = requestForwarder;
        }

        /*
         *  TODO: make proxy error logs saveable in file
         */
        public async Task InvokeAsync(HttpContext context)
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

            // TODO: load balancer

            // forward request with resolved route
            var response = await _requestForwarder.ForwardAsync(context, resolvedRoute, ct);

            // forward response
            await _responseForwarder.ForwardAsync(context, response, ct);
        }
    }
}
