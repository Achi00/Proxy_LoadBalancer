namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class BackendOption
    {
        public required string Url { get; set; }
        public int Weight { get; set; } = 1;

        // Runtime properties (not from config)
        public int ActiveConnections { get; set; }
        public bool IsHealthy { get; set; } = true;
    }
}
