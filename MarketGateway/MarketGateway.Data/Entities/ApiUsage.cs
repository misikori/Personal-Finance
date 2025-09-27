namespace MarketGateway.Data.Entities;


public class ApiUsage
{
    public int Id { get; set; }
    public string Vendor { get; set; } = default!;
    public DateTime Date { get; set; }        
    public int CallsMade { get; set; }
}