

using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Entities;


public class ParseFailure
{
    public long Id { get; set; }
    public string Vendor { get; set; } = default!;
    public DataType Type { get; set; }
    public string PrimaryIdentifier { get; set; } = default!;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string Error { get; set; } = default!;
}