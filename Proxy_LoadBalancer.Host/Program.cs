using Proxy_LoadBalancer.Host.Infrastructure.Extensions;
using Proxy_LoadBalancer.Host.Middleware;
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

builder.Host.AddSerilogConfig();

builder.Services.AddSingleton<ProxyMiddleware>();
builder.Services.AddSingleton<ConfigRouteResolver>();
builder.Services.AddSingleton<HttpRequestForwarder>();
builder.Services.AddSingleton<HttpResponseForwarder>();
builder.Services.AddSingleton<PassiveHealthTracker>();


var app = builder.Build();

app.UseMiddleware<ProxyExceptionHandlerMiddleware>();
app.UseMiddleware<ProxyMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();
