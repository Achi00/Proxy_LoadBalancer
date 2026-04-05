using Proxy_LoadBalancer.Infrastructure.Options.Health;
using Proxy_LoadBalancer.Infrastructure.Routing;

namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class RouteOption
    {
        public string RouteId { get; set; }
        public string ClusterId { get; set; }
        public MatchOption Match { get; set; }
        public RouteTransform? Transform { get; set; }
        public int? Priority { get; set; }
    }
}
