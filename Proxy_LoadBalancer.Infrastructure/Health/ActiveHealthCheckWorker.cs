using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Infrastructure.Options;
using Proxy_LoadBalancer.Infrastructure.Options.Health;

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
            while (!ct.IsCancellationRequested)
            {
                foreach (var cluster in _options.Value.Clusters.Values)
                {
                    // resolve health check config
                    var healthCheck = cluster.HealthCheck ?? _options.Value.Defaults?.HealthCheck;

                    // skip if no health check is enabled
                    if (healthCheck?.Enabled != true)
                    {
                        continue;
                    }

                    foreach (var destination in cluster.Destinations)
                    {
                        await ProbeAsync(destination, healthCheck, ct);
                    }
                }

                // get active check interval from config file
                var interval = _options.Value.Clusters.Values
                    .Select(c => c.HealthCheck ?? _options.Value.Defaults?.HealthCheck)
                    .Where(h => h?.Enabled == true)
                    .Min(h => h?.IntervalSeconds ?? 10);

                await Task.Delay(TimeSpan.FromSeconds(interval), ct);
            }
        }

        private async Task ProbeAsync(DestinationOption destination, HealthCheckOption healthCheck, CancellationToken ct)
        {
            try
            {
                var client = _factory.CreateClient("health-check");
                // set default /health if no explicit path defined in configs
                var url = destination.Address.TrimEnd('/') + (healthCheck.Path ?? "/health");
                var response = await client.GetAsync(url, ct);

                if (response.IsSuccessStatusCode)
                {
                    _healthTracker.RecordSuccess(destination.Address);
                    _logger.LogInformation($"Health probe {destination.Address}: healthy");
                }
                else
                {
                    _healthTracker.RecordFailure(destination.Address);
                    _logger.LogWarning($"Health probe {destination.Address}: unhealthy (HTTP {response.StatusCode})");
                }
            }
            catch (TaskCanceledException)
            {
                // triggered by timeout, expected error
                _healthTracker.RecordFailure(destination.Address);
                _logger.LogWarning($"Health probe {destination.Address}: timed out — marked unhealthy");
            }
            catch (HttpRequestException ex)
            {
                // connection refused or unreachable
                _healthTracker.RecordFailure(destination.Address);
                _logger.LogWarning($"Health probe {destination.Address}: unreachable — {ex.Message}");
            }
            catch (Exception ex)
            {
                // unexpected err
                _healthTracker.RecordFailure(destination.Address);
                _logger.LogError(ex, $"Health probe {destination.Address}: failed");
            }
        }
    }
}
