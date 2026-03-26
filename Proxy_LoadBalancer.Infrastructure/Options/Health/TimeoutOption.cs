namespace Proxy_LoadBalancer.Infrastructure.Options.Health
{
    public class TimeoutOption
    {
        public int ConnectSeconds { get; set; }
        public int ReadSeconds { get; set; }
        public int WriteSeconds { get; set; }
    }
}
