using Proxy_LoadBalancer.Infrastructure.Options.Health;

namespace Proxy_LoadBalancer.Infrastructure.Options.Defaults
{
    public class ProxyDefaults
    {
        public string LoadBalancing { get; set; }
        public string PathMatch { get; set; }
        public TimeoutOption Timeouts { get; set; }
        public HealthCheckOption HealthCheck { get; set; }
        public List<string> ForwardHeaders { get; set; }
    }
}
