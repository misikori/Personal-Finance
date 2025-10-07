namespace MarketGateway.Data;

public class StorageOptions
{
    public string Mode { get; set; } = "Database";   
    public string DatabasePath { get; set; } = "MarketGateway.Data/DataStorage/market.db";
    public string RootDirectory { get; set; } = "MarketGateway.Data/DataStorage";
    public string ConnectionString { get; set; } = "MarketGateway.Data/DataStorage/market.db";
}