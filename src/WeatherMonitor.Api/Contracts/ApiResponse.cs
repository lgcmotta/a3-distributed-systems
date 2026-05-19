namespace WeatherMonitor.Api.Contracts;

public record ApiResponse<TResponse>(TResponse Data);

public record PagedApiResponse<TResponse>(TResponse Data, PagedResponse Pagination) : ApiResponse<TResponse>(Data);