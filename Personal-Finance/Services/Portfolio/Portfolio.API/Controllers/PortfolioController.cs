using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Core.DTOs;
using Portfolio.Core.Services;
using Portfolio.Core.Repositories;

namespace Portfolio.API.Controllers;

/// <summary>
/// Portfolio management API - buy/sell stocks, view portfolio, get prices
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly IMarketDataService _marketDataService;
    private readonly IPredictionService _predictionService;
    private readonly IPortfolioRepository _repository;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(
        IPortfolioService portfolioService,
        IMarketDataService marketDataService,
        IPredictionService predictionService,
        IPortfolioRepository repository,
        ILogger<PortfolioController> logger)
    {
        _portfolioService = portfolioService;
        _marketDataService = marketDataService;
        _predictionService = predictionService;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Buy stocks for authenticated user (userId from JWT token)
    /// </summary>
    /// <param name="request">Buy request containing symbol and quantity (username ignored, userId from JWT)</param>
    /// <returns>Transaction details including price and total cost</returns>
    /// <response code="200">Stock purchased successfully</response>
    /// <response code="400">Invalid request (insufficient budget, invalid symbol, etc.)</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    [HttpPost("buy")]
    [ProducesResponseType(typeof(Core.Entities.Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BuyStock([FromBody] BuyStockRequest request)
    {
        try
        {
            // Extract userId from JWT token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                var allClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                _logger.LogError("Buy request REJECTED: No userId found. Claims: {Claims}", allClaims);
                return Unauthorized(new { 
                    error = "User ID not found in token. Please re-authenticate."
                });
            }
            
            // Set userId from JWT token (any value sent in request body is ignored)
            request.UserId = userId;

            // Input validation
            if (string.IsNullOrWhiteSpace(request.Symbol))
                return BadRequest(new { error = "Stock symbol is required" });

            if (string.IsNullOrWhiteSpace(request.WalletId))
                return BadRequest(new { error = "Wallet ID is required" });

            if (request.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than zero" });

            _logger.LogInformation("Received BUY request from UserId {UserId} for {Quantity} shares of {Symbol}", 
                userId, request.Quantity, request.Symbol);

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
    /// Sell stocks from authenticated user's portfolio (userId from JWT token)
    /// </summary>
    /// <param name="request">Sell request containing symbol and quantity (username ignored, userId from JWT)</param>
    /// <returns>Transaction details including sale price and gain/loss</returns>
    /// <response code="200">Stock sold successfully</response>
    /// <response code="400">Invalid request (insufficient shares, user doesn't own stock, etc.)</response>
    /// <response code="401">Unauthorized - valid JWT token required</response>
    [HttpPost("sell")]
    [ProducesResponseType(typeof(Core.Entities.Transaction), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SellStock([FromBody] SellStockRequest request)
    {
        try
        {
            // Extract userId from JWT token
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                var availableClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                _logger.LogWarning("Sell request rejected: No userId found in JWT token. Available claims: {Claims}", availableClaims);
                return Unauthorized(new { error = "User ID not found in token. Please re-authenticate." });
            }

            // Set userId from JWT token (any value sent in request body is ignored)
            request.UserId = userId;

            // Input validation
            if (string.IsNullOrWhiteSpace(request.Symbol))
                return BadRequest(new { error = "Stock symbol is required" });

            if (string.IsNullOrWhiteSpace(request.WalletId))
                return BadRequest(new { error = "Wallet ID is required" });

            if (request.Quantity <= 0)
                return BadRequest(new { error = "Quantity must be greater than zero" });

            _logger.LogInformation("Received SELL request from UserId {UserId} for {Quantity} shares of {Symbol}", 
                userId, request.Quantity, request.Symbol);

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
    /// All values are converted to the specified base currency for accurate totals
    /// </summary>
    /// <param name="username">Username of the portfolio owner</param>
    /// <param name="baseCurrency">Base currency for calculations (default: USD)</param>
    /// <returns>Portfolio summary with all positions and performance metrics</returns>
    /// <response code="200">Portfolio summary retrieved successfully</response>
    [HttpGet("summary/{username}")]
    [ProducesResponseType(typeof(PortfolioSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPortfolioSummary(string username, [FromQuery] string baseCurrency = "USD")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { error = "Username is required" });

            _logger.LogInformation("Fetching portfolio summary for {Username} in {BaseCurrency}", username, baseCurrency);

            var summary = await _portfolioService.GetPortfolioSummaryAsync(username, baseCurrency);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching portfolio summary");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get transaction history for a user
    /// Returns chronological list of all buy/sell transactions
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>List of transactions ordered by date (newest first)</returns>
    /// <response code="200">Transaction history retrieved successfully</response>
    [HttpGet("transactions/{username}")]
    [ProducesResponseType(typeof(List<Core.Entities.Transaction>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { error = "Username is required" });

            _logger.LogInformation("Fetching transaction history for {Username}", username);

            var transactions = await _repository.GetUserTransactionsAsync(username);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get portfolio distribution for pie chart visualization
    /// Returns percentage breakdown of each stock holding by current value
    /// Perfect for rendering pie charts in the frontend
    /// All values are converted to the specified base currency for accurate comparison
    /// </summary>
    /// <param name="username">Username of the portfolio owner</param>
    /// <param name="baseCurrency">Base currency for calculations (default: USD)</param>
    /// <returns>Portfolio distribution with percentages and suggested colors</returns>
    /// <response code="200">Distribution data retrieved successfully</response>
    [HttpGet("distribution/{username}")]
    [ProducesResponseType(typeof(PortfolioDistributionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPortfolioDistribution(string username, [FromQuery] string baseCurrency = "USD")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { error = "Username is required" });

            _logger.LogInformation("Fetching portfolio distribution for {Username} in {BaseCurrency}", username, baseCurrency);

            var distribution = await _portfolioService.GetPortfolioDistributionAsync(username, baseCurrency);
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching portfolio distribution");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get ML-powered stock recommendations with buy/sell/hold suggestions
    /// </summary>
    /// <param name="symbols">Comma-separated stock symbols to analyze (e.g., "AAPL,TSLA,MSFT")</param>
    /// <returns>ML-based buy, sell, and hold recommendations sorted by strongest signals</returns>
    /// <response code="200">Recommendations generated successfully</response>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(StockRecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecommendations([FromQuery] string symbols)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbols))
            {
                return BadRequest(new { error = "Please provide symbols parameter (e.g., ?symbols=AAPL,TSLA,MSFT)" });
            }

            var symbolList = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToUpper())
                .Distinct()
                .ToList();

            if (symbolList.Count == 0)
            {
                return BadRequest(new { error = "No valid symbols provided" });
            }

            _logger.LogInformation("Generating ML recommendations for symbols: {Symbols}", string.Join(", ", symbolList));

            var recommendations = await _predictionService.GetRecommendationsAsync(symbolList);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current market price for a stock
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL", "TSLA", "MSFT")</param>
    /// <returns>Current price information</returns>
    [HttpGet("price/{symbol}")]
    [ProducesResponseType(typeof(StockPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStockPrice(string symbol)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest(new { error = "Stock symbol is required" });

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
    /// Get candlestick chart data for a stock
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <param name="days">Number of days of data (default: 30)</param>
    /// <returns>Candlestick data ready for charting libraries</returns>
    [HttpGet("candlestick/{symbol}")]
    [ProducesResponseType(typeof(CandlestickDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCandlestickData(string symbol, [FromQuery] int days = 30)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest(new { error = "Stock symbol is required" });

            if (days <= 0)
                return BadRequest(new { error = "Days must be greater than zero" });

            _logger.LogInformation("Fetching {Days} days of candlestick data for {Symbol}", days, symbol);

            var data = await _marketDataService.GetCandlestickDataAsync(symbol, days);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching candlestick data for {Symbol}", symbol);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get ML price prediction
    /// </summary>
    /// <param name="symbol">Stock symbol to predict (e.g., "AAPL")</param>
    /// <returns>ML predicted price target with confidence level</returns>
    [HttpGet("predict/{symbol}")]
    [ProducesResponseType(typeof(PricePredictionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPricePrediction(string symbol)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return BadRequest(new { error = "Stock symbol is required" });

            _logger.LogInformation("Generating ML price prediction for {Symbol}", symbol);

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
