using System.Net;

namespace MarketGateway.Shared.API;

public sealed record ApiResult<T>
{
    public required bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public string? VendorError { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public DateTimeOffset? RetryAfter { get; init; }
    public IReadOnlyDictionary<string, object>? Meta { get; init; }

    public static ApiResult<T> Ok(T data, HttpStatusCode? status = null, IReadOnlyDictionary<string, object>? meta = null)
        => new() { Success = true, Data = data, StatusCode = status, Meta = meta };

    public static ApiResult<T> Fail(
        string error,
        HttpStatusCode? status = null,
        string? vendorError = null,
        DateTimeOffset? retryAfter = null,
        IReadOnlyDictionary<string, object>? meta = null)
        => new() { Success = false, Error = error, StatusCode = status, VendorError = vendorError, RetryAfter = retryAfter, Meta = meta };
}