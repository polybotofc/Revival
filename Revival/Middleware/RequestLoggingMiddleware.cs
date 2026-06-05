using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;

namespace Revival.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// Provides detailed logging for debugging and monitoring.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // Log request
        var request = context.Request;
        var requestLog = new
        {
            RequestId = requestId,
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
            UserAgent = request.Headers.UserAgent.ToString()
        };

        _logger.LogInformation(
            "Request {RequestId}: {Method} {Path}{Query} from {RemoteIp}",
            requestId,
            request.Method,
            request.Path,
            request.QueryString,
            context.Connection.RemoteIpAddress);

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            
            // Log response
            _logger.LogInformation(
                "Response {RequestId}: {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(
                ex,
                "Request {RequestId} failed with exception after {ElapsedMs}ms",
                requestId,
                stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}

/// <summary>
/// Extension methods for adding request logging middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}