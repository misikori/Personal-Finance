namespace MarketGateway.Data.Entities;

public class OhlcvSeriesEntity
{
    public int Id { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string? Exchange { get; set; }
    public string? Currency { get; set; }

    public int Granularity { get; set; }  
    public int Adjustment  { get; set; }  
    public bool Partial { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
}