namespace Proxy_LoadBalancer.Host.Middleware
{
    public class ProxyExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ProxyExceptionHandlerMiddleware> _logger;

        public ProxyExceptionHandlerMiddleware(
            RequestDelegate next, 
            ILogger<ProxyExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // client disconnected or operation canceled while executing, not logging as error
                _logger.LogDebug("Client disconnected: {Method} {Path}", context.Request.Method, context.Request.Path);
                // response already abandoned nothing to write
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Upstream unreachable: {Method} {Path}", context.Request.Method, context.Request.Path);
                await TryWriteErrorAsync(context, 502, "Bad Gateway");
            }
            catch (TaskCanceledException ex)
            {
                // upstream timed out or Task cancelation (HttpClient cancels the request, not client disconnect)
                // HttpClient throws TaskCanceledException not TimeoutException
                _logger.LogWarning(ex, "Upstream timed out: {Method} {Path}", context.Request.Method, context.Request.Path);
                await TryWriteErrorAsync(context, 504, "Gateway Timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled proxy error: {Method} {Path}", context.Request.Method, context.Request.Path);
                await TryWriteErrorAsync(context, 500, "Internal Server Error");
            }
        }

        private static async Task TryWriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            if (context.Response.HasStarted)
            {
                // if headers already sent we can't change the status code, ASP.NET already flushed headers
                // abort the connection so the client
                // knows something went wrong rather than getting a truncated body
                // similar "Immediate Abort" Mechanism as Microsoft YARP
                context.Abort();
                return;
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(message);
        }
    }
}
