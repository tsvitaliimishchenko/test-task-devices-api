using System.Net;
using System.Text.Json;
using DevicesApi.Domain.Exceptions;

namespace DevicesApi.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            DeviceNotFoundException notFoundEx =>
                (HttpStatusCode.NotFound, notFoundEx.Message),

            DeviceInUseException inUseEx =>
                (HttpStatusCode.Conflict, inUseEx.Message),

            InvalidDeviceStateException invalidStateEx =>
                (HttpStatusCode.BadRequest, invalidStateEx.Message),

            DomainException domainEx =>
                (HttpStatusCode.Conflict, domainEx.Message),

            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "Domain exception: {Message}", exception.Message);

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { message }, JsonOptions);
        return context.Response.WriteAsync(result);
    }
}
