namespace Proxy_LoadBalancer.Domain.Entity
{
    public class BackendConfig
    {
        public required string Url { get; set; }
        public int Weight { get; set; } = 1;

        // Runtime properties (not from config)
        public int ActiveConnections { get; set; }
        public bool IsHealthy { get; set; } = true;
    }
}
