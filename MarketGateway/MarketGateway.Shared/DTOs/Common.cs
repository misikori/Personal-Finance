namespace MarketGateway.Shared.DTOs;


public enum DataType { Quote = 1, OHLCV = 2 }
public enum BarGranularity { Unspecified = 0, M1, M5, H1, D1, W1 }
public enum PriceAdjustment { Unspecified = 0, None, Splits, SplitsDivs }

public sealed record IdentifierDto(string Symbol, string? Exchange = null, string? AssetType = null);
public sealed record TimeRangeDto(DateTimeOffset? Start, DateTimeOffset? End);