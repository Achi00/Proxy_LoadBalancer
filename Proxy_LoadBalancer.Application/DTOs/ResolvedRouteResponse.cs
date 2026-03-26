namespace Proxy_LoadBalancer.Application.DTOs
{
    public class ResolvedRouteResponse
    {
        public string TargetUrl { get; set; }
        public string ClusterId { get; set; }
        public string RouteId { get; set; }
    }
}
