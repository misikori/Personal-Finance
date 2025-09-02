namespace MarketGateway.Shared.DTOs;

public sealed record FetchGate(bool Allowed, string? Reason, DateTimeOffset? RetryAfter);