using MediatR;
using WeatherMonitor.Api.Infrastructure.Extensions;

namespace WeatherMonitor.Api.Behaviors;

public partial class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var behaviorTypeName = typeof(LoggingBehavior<TRequest, TResponse>).GetGenericTypeName();

        var requestTypeName = typeof(TRequest).GetGenericTypeName();

        try
        {
            LogHandlingRequest(behaviorTypeName, requestTypeName);

            var response = await next(cancellationToken).ConfigureAwait(false);

            LogRequestHandled(behaviorTypeName, requestTypeName);

            return response;
        }
        catch (Exception exception)
        {
            LogRequestHandlingFailed(exception, behaviorTypeName, requestTypeName);

            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Behavior}] - Handling request of type {Request}")]
    private partial void LogHandlingRequest(string behavior, string request);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Behavior}] - Request of type {Request} handled successfully")]
    private partial void LogRequestHandled(string behavior, string request);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{Behavior}] - An exception occurred while handling request of type {Request}")]
    private partial void LogRequestHandlingFailed(Exception exception, string behavior, string request);
}