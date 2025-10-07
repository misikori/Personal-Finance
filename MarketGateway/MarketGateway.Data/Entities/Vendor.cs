namespace MarketGateway.Data.Entities;

public class Vendor
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;     
    public bool Enabled { get; set; } = true;
    
    public int PerMinuteLimit { get; set; } = 0; 
    public int PerDayLimit { get; set; } = 0;         

    public string? Notes { get; set; }
}