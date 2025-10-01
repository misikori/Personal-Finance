using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.DTOs;
using Portfolio.Core.Services;

namespace Portfolio.API.Controllers;

/// <summary>
/// Portfolio management API - buy/sell stocks, view portfolio, get prices
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly IMarketDataService _marketDataService;
    private readonly IPredictionService _predictionService;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(
        IPortfolioService portfolioService,
        IMarketDataService marketDataService,
        IPredictionService predictionService,
        ILogger<PortfolioController> logger)
    {
        _portfolioService = portfolioService;
        _marketDataService = marketDataService;
        _predictionService = predictionService;
        _logger = logger;
    }

    /// <summary>
    /// Buy stocks for a user
    /// </summary>
    /// <param name="request">Buy request containing username, symbol, and quantity</param>
    /// <returns>Transaction details including price and total cost</returns>
    /// <response code="200">Stock purchased successfully</response>
    /// <response code="400">Invalid request (insufficient budget, invalid symbol, etc.)</response>
    [HttpPost("buy")]
    [ProducesResponseType(typeof(Core.Entities.Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BuyStock([FromBody] BuyStockRequest request)
    {
        try
        {
            _logger.LogInformation("Received BUY request from {Username} for {Quantity} shares of {Symbol}", 
                request.Username, request.Quantity, request.Symbol);

            var transaction = await _portfolioService.BuyStockAsync(request);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BUY request");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sell stocks from user's portfolio
    /// </summary>
    /// <param name="request">Sell request containing username, symbol, and quantity</param>
    /// <returns>Transaction details including sale price and gain/loss</returns>
    /// <response code="200">Stock sold successfully</response>
    /// <response code="400">Invalid request (insufficient shares, user doesn't own stock, etc.)</response>
    [HttpPost("sell")]
    [ProducesResponseType(typeof(Core.Entities.Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SellStock([FromBody] SellStockRequest request)
    {
        try
        {
            _logger.LogInformation("Received SELL request from {Username} for {Quantity} shares of {Symbol}", 
                request.Username, request.Quantity, request.Symbol);

            var transaction = await _portfolioService.SellStockAsync(request);
            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SELL request");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get complete portfolio summary for a user
    /// Shows all positions with current values, gains/losses, and overall portfolio performance
    /// </summary>
    /// <param name="username">Username of the portfolio owner</param>
    /// <returns>Portfolio summary with all positions and performance metrics</returns>
    /// <response code="200">Portfolio summary retrieved successfully</response>
    [HttpGet("summary/{username}")]
    [ProducesResponseType(typeof(PortfolioSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPortfolioSummary(string username)
    {
        try
        {
            _logger.LogInformation("Fetching portfolio summary for {Username}", username);

            var summary = await _portfolioService.GetPortfolioSummaryAsync(username);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching portfolio summary");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current market price for a stock
    /// Fetches real-time price data from MarketGateway
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL", "TSLA", "MSFT")</param>
    /// <returns>Current price information including open, high, low, volume</returns>
    /// <response code="200">Price retrieved successfully</response>
    /// <response code="400">Invalid symbol or market data unavailable</response>
    [HttpGet("price/{symbol}")]
    [ProducesResponseType(typeof(StockPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStockPrice(string symbol)
    {
        try
        {
            _logger.LogInformation("Fetching current price for {Symbol}", symbol);

            var price = await _marketDataService.GetCurrentPriceAsync(symbol);
            return Ok(price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching price for {Symbol}", symbol);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get price prediction for a stock
    /// Uses historical data and trend analysis to predict future price movement
    /// </summary>
    /// <param name="symbol">Stock symbol to predict (e.g., "AAPL")</param>
    /// <returns>Price prediction with confidence level and methodology</returns>
    /// <response code="200">Prediction generated successfully</response>
    /// <response code="400">Insufficient historical data or invalid symbol</response>
    [HttpGet("predict/{symbol}")]
    [ProducesResponseType(typeof(PricePredictionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPricePrediction(string symbol)
    {
        try
        {
            _logger.LogInformation("Generating price prediction for {Symbol}", symbol);

            var prediction = await _predictionService.PredictPriceAsync(symbol);
            return Ok(prediction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prediction for {Symbol}", symbol);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new { status = "healthy", service = "Portfolio.API", timestamp = DateTime.UtcNow });
    }
}


