namespace MarketGateway.Shared.API;

public sealed record FetchGate(bool Allowed, string? Reason, DateTimeOffset? RetryAfter);