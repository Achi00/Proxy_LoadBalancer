using Proxy_LoadBalancer.Domain.Enum;

namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class RouteOption
    {
        public required string Path { get; set; }
        public List<BackendOption> Backends { get; set; } = new();
        public LoadBalancingStrategy LoadBalancing { get; set; } = LoadBalancingStrategy.RoundRobin;
        public string? HealthCheck { get; set; }
    }
}
