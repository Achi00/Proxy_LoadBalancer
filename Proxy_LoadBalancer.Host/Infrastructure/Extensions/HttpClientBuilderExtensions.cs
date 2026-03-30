using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Collections.Immutable;

namespace Proxy_LoadBalancer.Host.Infrastructure.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        private static readonly ImmutableHashSet<HttpMethod> IdempotentMethods =
        [
            HttpMethod.Get,
            HttpMethod.Head,
            HttpMethod.Options
        ];

        public static IHttpClientBuilder AddProxyResiliencePolicy(
            this IHttpClientBuilder builder,
            int perAttemptTimeoutSeconds = 10,
            int absoluteTimeoutSeconds = 25)
        {
            builder.ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(absoluteTimeoutSeconds + 5);
                });
                
                builder.AddResilienceHandler("proxy-pipeline", pipeline =>
                {
                    pipeline
                        .AddTimeout(TimeSpan.FromSeconds(absoluteTimeoutSeconds))
                        .AddRetry(new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = 2,
                            Delay = TimeSpan.FromMilliseconds(200),
                            BackoffType = DelayBackoffType.Linear,
                            ShouldHandle = args =>
                            {
                                var method = args.Outcome.Result?.RequestMessage?.Method;

                                if (method is null || !IdempotentMethods.Contains(method))
                                {
                                    return ValueTask.FromResult(false);
                                }

                                if (args.Outcome.Exception is HttpRequestException
                                    or Polly.Timeout.TimeoutRejectedException)
                                {
                                    return ValueTask.FromResult(true);
                                }

                                return ValueTask.FromResult(
                                    args.Outcome.Result is { } r && (int)r.StatusCode >= 500);
                            },
                            OnRetry = args =>
                            {
                                var method = args.Outcome.Result?.RequestMessage?.Method;
                                var uri = args.Outcome.Result?.RequestMessage?.RequestUri;
                                var reason = args.Outcome.Exception?.GetType().Name
                                          ?? $"{(int)args.Outcome.Result!.StatusCode}";
                                // TODO: inject and use ILogger properly
                                Console.WriteLine(
                                    $"[Retry {args.AttemptNumber}] {method} {uri} — {reason}");
                                return ValueTask.CompletedTask;
                            }
                        })
                        .AddTimeout(TimeSpan.FromSeconds(perAttemptTimeoutSeconds));
                });

            return builder;
        }
    }
}
