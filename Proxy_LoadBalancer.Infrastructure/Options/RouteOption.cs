using Proxy_LoadBalancer.Infrastructure.Options.Health;

namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class RouteOption
    {
        public string RouteId { get; set; }
        public string ClusterId { get; set; }
        public MatchOption Match { get; set; }
        public HealthCheckOption HealthCheck { get; set; }
        public int? Priority { get; set; }
    }
}
