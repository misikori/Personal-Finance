using MarketGateway.Data.Interfaces;

namespace MarketGateway.Data.Time;

public class FixedAppDate: IAppDate
{
    public FixedAppDate(DateOnly date) => Today = date;
    public DateOnly Today { get; }
}