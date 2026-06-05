using System.Net;
using System.Text.Json;
using FluentValidation;
using OpenFinancialExchange.Domain.Exceptions;

namespace OpenFinancialExchange.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain exception: {Message}", ex.Message);
            await WriteJsonResponse(context, HttpStatusCode.BadRequest, new
            {
                type    = "DomainException",
                title   = "Business rule violation",
                status  = 400,
                detail  = ex.Message
            });
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation exception");
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await WriteJsonResponse(context, HttpStatusCode.UnprocessableEntity, new
            {
                type   = "ValidationException",
                title  = "One or more validation errors occurred",
                status = 422,
                errors
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteJsonResponse(context, HttpStatusCode.InternalServerError, new
            {
                type   = "InternalServerError",
                title  = "An unexpected error occurred",
                status = 500,
                detail = "Please contact support if the problem persists."
            });
        }
    }

    private static Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, object body)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}
