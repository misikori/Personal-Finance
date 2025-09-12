using System.Net;
using MarketGateway.Shared.DTOs;

namespace MarketGateway.Data.Entities;

public class ApiCall
{
    public long Id { get; set; }
    public string Vendor { get; set; } = default!;
    public DataType Type { get; set; }
    public string Identifier { get; set; } = default!;
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public bool Succeeded { get; set; }
    public HttpStatusCode? HttpStatus { get; set; }
    public int? ResponseBytes { get; set; }
    public int? ParseLatencyMs { get; set; }
    
    public string? Error { get; set; }
    public int StoredRows { get; set; }
}