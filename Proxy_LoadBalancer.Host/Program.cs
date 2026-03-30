using Polly;
using Proxy_LoadBalancer.Host.Infrastructure.Extensions;
using Proxy_LoadBalancer.Host.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddProxyConfig(builder.Configuration);

// TODO: (GET, HEAD) automatically, never blindly retry POST
builder.Services.AddHttpClient("proxy", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(200 * attempt))
);


var app = builder.Build();

app.UseMiddleware<ProxyExceptionHandlerMiddleware>();
app.UseMiddleware<ProxyMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();
