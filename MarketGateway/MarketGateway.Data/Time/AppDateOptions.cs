namespace MarketGateway.Data.Time;

public sealed class AppDateOptions
{
    /// <summary>"Utc" or "Fixed"</summary>
    public string Mode { get; set; } = "Utc";
    /// <summary>Only used when Mode == "Fixed" (yyyy-MM-dd)</summary>
    public string? FixedDate { get; set; }
}