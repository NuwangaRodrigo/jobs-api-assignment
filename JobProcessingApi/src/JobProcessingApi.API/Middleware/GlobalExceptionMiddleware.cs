using System.Net;
using System.Text.Json;

namespace JobProcessingApi.API.Middleware;

//<summary>
//Global exception handling middleware
  
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        // Customize response based on exception type
        switch (exception)
        {
            case ArgumentNullException or ArgumentException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;
            case NotSupportedException:
                code = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
        }

        var result = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)code,
            timestamp = DateTime.UtcNow
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

//<summary>
//Extension method for registering the middleware
  
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
