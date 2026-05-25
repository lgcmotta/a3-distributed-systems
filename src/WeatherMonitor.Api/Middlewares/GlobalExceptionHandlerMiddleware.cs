using Microsoft.AspNetCore.Diagnostics;

namespace WeatherMonitor.Api.Middlewares;

internal sealed class GlobalExceptionHandlerMiddleware(IExceptionHandler handler) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (OperationCanceledException exception) when (exception.CancellationToken == context.RequestAborted)
        {
            throw;
        }
        catch (Exception exception)
        {
            await handler.TryHandleAsync(context, exception, context.RequestAborted);
        }
    }
}