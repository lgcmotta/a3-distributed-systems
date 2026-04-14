namespace WeatherMonitor.Api.Shared;

public record ApiResponse<TResponse>(TResponse Data);

public record PagedApiResponse<TResponse>(TResponse Data, PagedResponseModel Pagination) : ApiResponse<TResponse>(Data);