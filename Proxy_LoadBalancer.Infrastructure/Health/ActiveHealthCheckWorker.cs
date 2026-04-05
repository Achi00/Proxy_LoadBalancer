using Microsoft.Extensions.Hosting;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Health
{
    public class ActiveHealthCheckWorker : BackgroundService
    {
        private readonly IHttpClientFactory _factory;
        private readonly PassiveHealthTracker _healthTracker;
        private readonly ProxyOption _options;

        public ActiveHealthCheckWorker(IHttpClientFactory factory, PassiveHealthTracker healthTracker, ProxyOption options)
        {
            _factory = factory;
            _healthTracker = healthTracker;
            _options = options;
        }
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                foreach (var cluster in _options.Clusters.Values)
                {
                    foreach (var destination in cluster.Destinations)
                    {
                        foreach (var route in _options.Routes.Where(r => r.ClusterId == cluster.Id))
                        {
                            await ProbeAsync(destination, route, ct);
                        }
                    }
                }

                var delaySeconds = _options.Routes
                    .Where(r => _options.Clusters.Values.Any(c => c.Id == r.ClusterId))
                    .Min(r => r.HealthCheck.IntervalSeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
            }
        }

        private async Task ProbeAsync(DestinationOption destination, RouteOption route, CancellationToken ct)
        {
            try
            {
                var client = _factory.CreateClient("proxy");
                var response = await client.GetAsync(destination.Address + route.HealthCheck.Path, ct);
                if (response.IsSuccessStatusCode)
                {
                    _healthTracker.RecordSuccess(destination.Address);
                }
                else
                {
                    _healthTracker.RecordFailure(destination.Address);
                }
            }
            catch
            {
                _healthTracker.RecordFailure(destination.Address);
            }
        }
    }
}
