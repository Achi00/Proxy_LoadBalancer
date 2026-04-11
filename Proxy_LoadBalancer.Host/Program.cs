using Proxy_LoadBalancer.Host.Infrastructure.Extensions;
using Proxy_LoadBalancer.Host.Middleware;
using Proxy_LoadBalancer.Infrastructure.Cache;
using Proxy_LoadBalancer.Infrastructure.Cache.Key;
using Proxy_LoadBalancer.Infrastructure.Cache.Policy;
using Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders;
using Proxy_LoadBalancer.Infrastructure.Health;
using Proxy_LoadBalancer.Infrastructure.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProxyConfig(builder.Configuration);

builder.Services
    .AddHttpClient("proxy")
    // TODO: for localhost testing
    //.ConfigurePrimaryHttpMessageHandler(() =>
    //   new HttpClientHandler
    //   {
    //       ServerCertificateCustomValidationCallback =
    //           HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    //   })
    .AddProxyResiliencePolicy(perAttemptTimeoutSeconds: 10, absoluteTimeoutSeconds: 25);

builder.Services
    .AddHttpClient("health-check", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(3);
    })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Host.AddSerilogConfig();

builder.Services.AddSingleton<ProxyMiddleware>();
builder.Services.AddSingleton<ConfigRouteResolver>();
builder.Services.AddSingleton<HttpRequestForwarder>();
builder.Services.AddSingleton<HttpResponseForwarder>();
builder.Services.AddSingleton<PassiveHealthTracker>();
builder.Services.AddHostedService<ActiveHealthCheckWorker>();
builder.Services.AddTransient<ResponseCacheMiddleware>();

builder.Services.AddTransient<ICachePolicy, CachePolicy>();
builder.Services.AddTransient<ICacheKeyProvider, KeyProvider>();
builder.Services.AddTransient<IResponseCacheStore, MemoryResponseCacheStore>();
builder.Services.AddTransient<ResponseCacheMiddleware>();

var app = builder.Build();

app.UseMiddleware<ProxyExceptionHandlerMiddleware>();
app.UseMiddleware<ResponseCacheMiddleware>();
app.UseMiddleware<ProxyMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();
