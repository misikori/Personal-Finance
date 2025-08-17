namespace MarketGateway.Shared.DTOs;

public class MarketDataResultFactory
{
    private static readonly Dictionary<DataType, Func<MarketDataResultBase>> _creators = new();
    
    static MarketDataResultFactory()
    {
        Register(DataType.Quote, () => new QuoteDto());
    }
    
    public static void Register(DataType dataType, Func<MarketDataResultBase> creator) => _creators[dataType] = creator;

    public static MarketDataResultBase Create(DataType dataType)
    {
        if (_creators.TryGetValue(dataType, out var factory))
            return factory();
        
        throw new NotSupportedException($"No result type registered for {dataType}");
    }
}