using System.Net;
using System.Text.Json;
using SmartAIAgent.Application.Common;

namespace SmartAIAgent.Api.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (ApplicationError exception)
        {
            await WriteErrorAsync(context, exception.Code, exception.Message, MapStatusCode(exception.Code));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request {Path}", context.Request.Path);
            await WriteErrorAsync(context, "UNEXPECTED_ERROR", "An unexpected error occurred.", HttpStatusCode.InternalServerError);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, string code, string message, HttpStatusCode statusCode)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(ApiResponse<object>.Fail(code, message));
        await context.Response.WriteAsync(payload);
    }

    private static HttpStatusCode MapStatusCode(string code) => code switch
    {
        "VALIDATION_ERROR" => HttpStatusCode.BadRequest,
        "REQUIREMENT_NOT_FOUND" => HttpStatusCode.NotFound,
        "AGENT_RUN_NOT_FOUND" => HttpStatusCode.NotFound,
        "INVALID_WORKFLOW_TRANSITION" => HttpStatusCode.Conflict,
        "AGENT_RUN_ALREADY_ACTIVE" => HttpStatusCode.Conflict,
        "AGENT_RUN_ALREADY_APPROVED" => HttpStatusCode.Conflict,
        "AGENT_RUN_ALREADY_REJECTED" => HttpStatusCode.Conflict,
        "APPROVAL_NOT_PENDING" => HttpStatusCode.Conflict,
        "REQUIREMENT_ANALYSIS_FAILED" => HttpStatusCode.BadGateway,
        _ => HttpStatusCode.BadRequest
    };
}
