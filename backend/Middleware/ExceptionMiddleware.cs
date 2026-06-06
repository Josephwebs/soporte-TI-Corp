using System.Net;
using System.Text.Json;

namespace TicketManager.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict operation: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error inesperado.");
            await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError, "Ocurrió un error inesperado en el servidor.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var response = new { error = message, statusCode = (int)statusCode };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
