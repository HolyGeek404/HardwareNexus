using Microsoft.AspNetCore.Diagnostics;
using ProductApi.Api.Services;

namespace ProductApi.Api.Extensions;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            System.Text.Json.JsonException => StatusCodes.Status400BadRequest,
            NotImplementedException => StatusCodes.Status501NotImplemented,
            _                       => StatusCodes.Status500InternalServerError
        };
        httpContext.Response.StatusCode = statusCode;
        
        var logLevel = statusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning;
        Logger.LogRequestFailed(logger, logLevel, exception, httpContext.Request.Method, httpContext.Request.Path, statusCode);
        
        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception
        });
    }
}