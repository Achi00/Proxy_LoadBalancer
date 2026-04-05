using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Health
{
    public class ActiveHealthCheckWorker : BackgroundService
    {
        private readonly IHttpClientFactory _factory;
        private readonly PassiveHealthTracker _healthTracker;
        private readonly IOptions<ProxyOption> _options;
        private readonly ILogger<ActiveHealthCheckWorker> _logger;

        public ActiveHealthCheckWorker(
            IHttpClientFactory factory, 
            PassiveHealthTracker healthTracker, 
            IOptions<ProxyOption> options,
            ILogger<ActiveHealthCheckWorker> logger)
        {
            _factory = factory;
            _healthTracker = healthTracker;
            _options = options;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("Active health check worker started");
            
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    foreach (var cluster in _options.Value.Clusters.Values)
                    {
                        foreach (var destination in cluster.Destinations)
                        {
                            foreach (var route in _options.Value.Routes.Where(r => r.ClusterId == cluster.Id))
                            {
                                await ProbeAsync(destination, route, ct);
                            }
                        }
                    }

                    var delaySeconds = _options.Value.Routes
                        .Where(r => _options.Value.Clusters.Values.Any(c => c.Id == r.ClusterId))
                        .Min(r => r.HealthCheck.IntervalSeconds);

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Active health check worker stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in health check worker");
                }
            }
        }

        private async Task ProbeAsync(DestinationOption destination, RouteOption route, CancellationToken ct)
        {
            try
            {
                var client = _factory.CreateClient("proxy");
                var url = destination.Address + route.HealthCheck.Path;
                
                var response = await client.GetAsync(url, ct);
                
                if (response.IsSuccessStatusCode)
                {
                    _healthTracker.RecordSuccess(destination.Address);
                    _logger.LogInformation(
                        "Health probe {Address}: {Status}", 
                        destination.Address, 
                        "healthy");
                }
                else
                {
                    _healthTracker.RecordFailure(destination.Address);
                    _logger.LogWarning(
                        "Health probe {Address}: {Status} (HTTP {StatusCode})", 
                        destination.Address, 
                        "unhealthy",
                        response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _healthTracker.RecordFailure(destination.Address);
                _logger.LogError(ex, "Health probe {Address}: {Status}", destination.Address, "failed");
            }
        }
    }
}
