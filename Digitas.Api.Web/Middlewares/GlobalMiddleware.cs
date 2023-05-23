using System.Diagnostics;
using System.Text.Json;
using Digitas.Api.Web.Extensions;
using Microsoft.AspNetCore.Http.Extensions;

namespace Digitas.Api.Web.Middlewares;

public sealed class GlobalMiddleware : IMiddleware
{
    private readonly ILogger<GlobalMiddleware> _logger;

    public static string ContentType => "application/json";

    public GlobalMiddleware(ILogger<GlobalMiddleware> logger)
        => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            if (context.Request.GetDisplayUrl().Contains("/health") || context.Request.GetDisplayUrl().Contains("/swagger"))
            {
                await next(context);
                return;
            }

            _logger.LogInformation("{method} request for \"{request}\" initialized.",
                                   context.Request.Method.ToUpper(),
                                   context.Request.GetDisplayUrl());

            var watch = new Stopwatch();
            watch.Start();

            await next(context);

            watch.Stop();

            _logger.LogInformation("{method} request for \"{request}\" completed in {elapsed}ms.",
                                   context.Request.Method.ToUpper(),
                                   context.Request.GetDisplayUrl(),
                                   watch.ElapsedMilliseconds);
        }
        catch (ArgumentNullException ex)
        {
            await CreateProblemOutputAsync("Invalid reference in an operation.", StatusCodes.Status400BadRequest, context, ex);
        }
        catch (ArgumentException ex)
        {
            await CreateProblemOutputAsync("Invalid paramter in an operation.", StatusCodes.Status400BadRequest, context, ex);
        }
        catch (AggregateException ex)
        {
            await CreateProblemOutputAsync("Error processing a set of instructions.", StatusCodes.Status500InternalServerError, context, ex);
        }
        catch (InvalidOperationException ex)
        {
            await CreateProblemOutputAsync("An invalid operation has occurred.", StatusCodes.Status500InternalServerError, context, ex);
        }
        catch (HttpRequestException ex)
        {
            await CreateProblemOutputAsync("An invalid HTTP operation has occurred.", StatusCodes.Status500InternalServerError, context, ex);
        }
        catch (NotImplementedException ex)
        {
            await CreateProblemOutputAsync("Operation not defined.", StatusCodes.Status500InternalServerError, context, ex);
        }
        catch (OperationCanceledException ex)
        {
            await CreateProblemOutputAsync("Internal tasks have been cancelled.", StatusCodes.Status500InternalServerError, context, ex);
        }
        catch (Exception ex)
        {
            await CreateProblemOutputAsync("Request issues found.", StatusCodes.Status500InternalServerError, context, ex);
        }
    }

    private static async Task CreateProblemOutputAsync(string title, int statusCode, HttpContext context, Exception ex)
    {
        var problem = HttpOutput.CreateProblemDetail(title,
                                                     statusCode,
                                                     context.Request.Path.Value,
                                                     ex.Message,
                                                     ex);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = ContentType;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
