using Grpc.Net.Client;
using MarketGateway.Contracts;
using Portfolio.Core.DTOs;
using Microsoft.Extensions.Logging;
using Google.Protobuf.WellKnownTypes;

namespace Portfolio.Core.Services;

/// <summary>
/// Implementation of market data service using MarketGateway gRPC
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly string _marketGatewayUrl;
    private readonly ILogger<MarketDataService> _logger;
    private readonly GrpcChannelOptions _channelOptions;

    public MarketDataService(string marketGatewayUrl, ILogger<MarketDataService> logger)
    {
        _marketGatewayUrl = marketGatewayUrl;
        _logger = logger;
        
        // Configure gRPC channel with timeouts
        _channelOptions = new GrpcChannelOptions
        {
            MaxRetryAttempts = 3,
            HttpHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            }
        };
    }

    /// <summary>
    /// Fetches current stock price from MarketGateway via gRPC
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL", "TSLA")</param>
    /// <returns>Current price information including open, high, low, volume</returns>
    public async Task<StockPriceResponse> GetCurrentPriceAsync(string symbol)
    {
        try
        {
            _logger.LogInformation("Fetching current price for {Symbol} from MarketGateway", symbol);
            
            using var channel = GrpcChannel.ForAddress(_marketGatewayUrl, _channelOptions);
            var client = new MarketDataGateway.MarketDataGatewayClient(channel);

            var request = new FetchRequest
            {
                Ctx = new RequestContext { ClientId = "PortfolioService" },
                DataType = DataType.Quote,
                Ids = { new Identifier { Symbol = symbol } }
            };

            // Call with 10 second timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var reply = await client.FetchAsync(request, cancellationToken: cts.Token);

            if (!reply.Ok || reply.Quote == null)
            {
                _logger.LogWarning("Failed to fetch quote for {Symbol}: {Error}", symbol, reply.Error);
                throw new Exception($"Failed to fetch price for {symbol}: {reply.Error}");
            }

            var quote = reply.Quote;
            
            return new StockPriceResponse
            {
                Symbol = symbol,
                Price = (decimal)quote.Price,
                Open = (decimal)quote.Open,
                High = (decimal)quote.High,
                Low = (decimal)quote.Low,
                PreviousClose = (decimal)quote.PrevClose,
                Volume = (decimal)quote.Volume,
                AsOf = quote.Asof?.ToDateTime() ?? DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Fetches historical OHLCV data for price prediction analysis
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="days">Number of days of historical data to fetch</param>
    /// <returns>List of closing prices for the requested period</returns>
    public async Task<List<decimal>> GetHistoricalPricesAsync(string symbol, int days)
    {
        try
        {
            _logger.LogInformation("Fetching {Days} days of historical data for {Symbol}", days, symbol);
            
            using var channel = GrpcChannel.ForAddress(_marketGatewayUrl, _channelOptions);
            var client = new MarketDataGateway.MarketDataGatewayClient(channel);

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var request = new FetchRequest
            {
                Ctx = new RequestContext { ClientId = "PortfolioService" },
                DataType = DataType.Ohlcv,
                Ids = { new Identifier { Symbol = symbol } },
                Range = new TimeRange
                {
                    Start = Timestamp.FromDateTime(startDate),
                    End = Timestamp.FromDateTime(endDate)
                }
            };

            // Call with 15 second timeout (historical data can be larger)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var reply = await client.FetchAsync(request, cancellationToken: cts.Token);

            if (!reply.Ok || reply.OhlcvSeries == null)
            {
                _logger.LogWarning("Failed to fetch historical data for {Symbol}: {Error}", symbol, reply.Error);
                throw new Exception($"Failed to fetch historical data for {symbol}: {reply.Error}");
            }

            // Extract closing prices from OHLCV bars
            var prices = reply.OhlcvSeries.Bars
                .Select(bar => (decimal)bar.Close)
                .ToList();

            _logger.LogInformation("Fetched {Count} historical prices for {Symbol}", prices.Count, symbol);
            
            return prices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical data for {Symbol}", symbol);
            throw;
        }
    }

    /// <summary>
    /// Fetches full OHLCV candlestick data for chart visualization
    /// Returns all data needed for candlestick charts (Open, High, Low, Close, Volume)
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="days">Number of days of candlestick data to fetch</param>
    /// <returns>Complete candlestick data ready for charting libraries</returns>
    public async Task<CandlestickDataResponse> GetCandlestickDataAsync(string symbol, int days)
    {
        try
        {
            _logger.LogInformation("Fetching {Days} days of candlestick data for {Symbol}", days, symbol);
            
            using var channel = GrpcChannel.ForAddress(_marketGatewayUrl, _channelOptions);
            var client = new MarketDataGateway.MarketDataGatewayClient(channel);

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var request = new FetchRequest
            {
                Ctx = new RequestContext { ClientId = "PortfolioService" },
                DataType = DataType.Ohlcv,
                Ids = { new Identifier { Symbol = symbol } },
                Range = new TimeRange
                {
                    Start = Timestamp.FromDateTime(startDate),
                    End = Timestamp.FromDateTime(endDate)
                }
            };

            // Call with 20 second timeout (candlestick data can be large)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var reply = await client.FetchAsync(request, cancellationToken: cts.Token);

            if (!reply.Ok || reply.OhlcvSeries == null)
            {
                _logger.LogWarning("Failed to fetch OHLCV data for {Symbol}: {Error}", symbol, reply.Error);
                throw new Exception($"Failed to fetch candlestick data for {symbol}: {reply.Error}");
            }

            // Convert OHLCV bars to candlestick format
            var candlesticks = reply.OhlcvSeries.Bars
                .Select(bar =>
                {
                    var open = (decimal)bar.Open;
                    var close = (decimal)bar.Close;
                    var change = close - open;
                    var changePercent = open != 0 ? (change / open) * 100 : 0;

                    return new Candlestick
                    {
                        Date = bar.Ts?.ToDateTime() ?? DateTime.UtcNow,
                        Open = open,
                        High = (decimal)bar.High,
                        Low = (decimal)bar.Low,
                        Close = close,
                        Volume = (decimal)bar.Volume,
                        Change = change,
                        ChangePercent = changePercent
                    };
                })
                .OrderBy(c => c.Date) // Ensure chronological order
                .ToList();

            var response = new CandlestickDataResponse
            {
                Symbol = symbol,
                StartDate = startDate,
                EndDate = endDate,
                Count = candlesticks.Count,
                Data = candlesticks
            };

            _logger.LogInformation("Fetched {Count} candlesticks for {Symbol}", candlesticks.Count, symbol);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching candlestick data for {Symbol}", symbol);
            throw;
        }
    }
}
