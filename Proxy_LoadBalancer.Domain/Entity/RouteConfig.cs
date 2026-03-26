namespace Proxy_LoadBalancer.Domain.Entity
{
    public class RouteConfig
    {
        public required string Path { get; set; }
        public List<BackendConfig> Backends { get; set; } = new();
        public LoadBalancingStrategy LoadBalancing { get; set; } = LoadBalancingStrategy.RoundRobin;
        public string? HealthCheck { get; set; }
    }
}
