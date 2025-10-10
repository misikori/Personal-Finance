using Portfolio.Core.DTOs;
using Portfolio.Core.Entities;
using Portfolio.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace Portfolio.Core.Services;

/// <summary>
/// Implementation of portfolio management operations
/// Handles buying, selling, and portfolio valuation
/// </summary>
public class PortfolioService : IPortfolioService
{
    private readonly IPortfolioRepository _repository;
    private readonly IMarketDataService _marketDataService;
    private readonly IBudgetService _budgetService;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IPortfolioRepository repository, 
        IMarketDataService marketDataService,
        IBudgetService budgetService,
        ICurrencyConverter currencyConverter,
        ILogger<PortfolioService> logger)
    {
        _repository = repository;
        _marketDataService = marketDataService;
        _budgetService = budgetService;
        _currencyConverter = currencyConverter;
        _logger = logger;
    }

    /// <summary>
    /// Executes a stock purchase
    /// 1. Gets current market price from MarketGateway
    /// 2. Validates and deducts funds from Budget service
    /// 3. Updates or creates position with new average price
    /// 4. Records transaction
    /// </summary>
    public async Task<Transaction> BuyStockAsync(BuyStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username is required");
        }

        _logger.LogInformation("Processing BUY order: {Username} buying {Quantity} shares of {Symbol}", 
            request.Username, request.Quantity, request.Symbol);

        // Get current market price
        var priceInfo = await _marketDataService.GetCurrentPriceAsync(request.Symbol);
        var totalCost = priceInfo.Price * request.Quantity;

        // Check if user has sufficient funds
        var hasEnoughMoney = await _budgetService.HasSufficientFundsAsync(request.Username, totalCost, priceInfo.Currency);
        if (!hasEnoughMoney)
        {
            throw new InvalidOperationException($"Insufficient funds. Required: {totalCost:F2} {priceInfo.Currency}");
        }

        // Deduct from budget
        var deductSuccess = await _budgetService.DeductFromBudgetAsync(request.Username, totalCost, priceInfo.Currency);
        if (!deductSuccess)
        {
            throw new InvalidOperationException($"Failed to deduct {totalCost:F2} {priceInfo.Currency} from budget");
        }

        // Get existing position or create new one
        var existingPosition = await _repository.GetPositionAsync(request.Username, request.Symbol);

        PortfolioPosition position;
        
        if (existingPosition != null)
        {
            // Update existing position - calculate new average price
            var totalShares = existingPosition.Quantity + request.Quantity;
            var totalValue = (existingPosition.Quantity * existingPosition.AveragePurchasePrice) + 
                           (request.Quantity * priceInfo.Price);
            
            position = new PortfolioPosition
            {
                Id = existingPosition.Id,
                Username = request.Username,
                Symbol = request.Symbol,
                Quantity = totalShares,
                AveragePurchasePrice = totalValue / totalShares,
                Currency = priceInfo.Currency,
                FirstPurchaseDate = existingPosition.FirstPurchaseDate,
                LastUpdated = DateTime.UtcNow
            };
        }
        else
        {
            // Create new position
            position = new PortfolioPosition
            {
                Username = request.Username,
                Symbol = request.Symbol,
                Quantity = request.Quantity,
                AveragePurchasePrice = priceInfo.Price,
                Currency = priceInfo.Currency,
                FirstPurchaseDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }

        await _repository.UpsertPositionAsync(position);

        // Record transaction
        var transaction = new Transaction
        {
            Username = request.Username,
            Symbol = request.Symbol,
            Type = "BUY",
            Quantity = request.Quantity,
            PricePerShare = priceInfo.Price,
            Currency = priceInfo.Currency
        };

        await _repository.AddTransactionAsync(transaction);

        _logger.LogInformation("BUY order completed: {TransactionId} - {Quantity} shares of {Symbol} at ${Price}", 
            transaction.Id, transaction.Quantity, transaction.Symbol, transaction.PricePerShare);

        return transaction;
    }

    /// <summary>
    /// Executes a stock sale
    /// 1. Verifies user owns enough shares
    /// 2. Gets current market price
    /// 3. Updates position quantity or removes if sold all
    /// 4. Records transaction and calculates gain/loss
    /// </summary>
    public async Task<Transaction> SellStockAsync(SellStockRequest request)
    {
        if (request.Quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username is required");
        }

        _logger.LogInformation("Processing SELL order: {Username} selling {Quantity} shares of {Symbol}", 
            request.Username, request.Quantity, request.Symbol);

        // Check if user owns this stock
        var existingPosition = await _repository.GetPositionAsync(request.Username, request.Symbol);
        
        if (existingPosition == null)
        {
            throw new InvalidOperationException($"User {request.Username} doesn't own any {request.Symbol} shares.");
        }

        if (existingPosition.Quantity < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient shares. User owns {existingPosition.Quantity}, trying to sell {request.Quantity}.");
        }

        // Get current market price
        var priceInfo = await _marketDataService.GetCurrentPriceAsync(request.Symbol);
        var saleProceeds = priceInfo.Price * request.Quantity;

        // Add proceeds to budget
        await _budgetService.AddToBudgetAsync(request.Username, saleProceeds, priceInfo.Currency);

        // Update or delete position
        if (existingPosition.Quantity == request.Quantity)
        {
            // Selling all shares - remove position
            await _repository.DeletePositionAsync(request.Username, request.Symbol);
            _logger.LogInformation("Removed position for {Username} - {Symbol} (sold all shares)", 
                request.Username, request.Symbol);
        }
        else
        {
            // Selling partial - update quantity
            existingPosition.Quantity -= request.Quantity;
            existingPosition.LastUpdated = DateTime.UtcNow;
            await _repository.UpsertPositionAsync(existingPosition);
        }

        // Record transaction
        var transaction = new Transaction
        {
            Username = request.Username,
            Symbol = request.Symbol,
            Type = "SELL",
            Quantity = request.Quantity,
            PricePerShare = priceInfo.Price,
            Currency = priceInfo.Currency
        };

        await _repository.AddTransactionAsync(transaction);

        var gainLoss = (priceInfo.Price - existingPosition.AveragePurchasePrice) * request.Quantity;
        
        _logger.LogInformation("SELL order completed: {TransactionId} - {Quantity} shares of {Symbol} at ${Price}, Gain/Loss: ${GainLoss}", 
            transaction.Id, transaction.Quantity, transaction.Symbol, transaction.PricePerShare, gainLoss);

        return transaction;
    }

    /// <summary>
    /// Generates complete portfolio summary with current market values
    /// Fetches real-time prices for all positions and calculates gains/losses
    /// All values are converted to the specified base currency for accurate totals
    /// </summary>
    public async Task<PortfolioSummaryResponse> GetPortfolioSummaryAsync(string username, string baseCurrency = "USD")
    {
        _logger.LogInformation("Generating portfolio summary for {Username} in {BaseCurrency}", username, baseCurrency);

        var positions = await _repository.GetUserPositionsAsync(username);

        if (!positions.Any())
        {
            return new PortfolioSummaryResponse
            {
                Username = username,
                BaseCurrency = baseCurrency,
                TotalInvested = 0,
                CurrentValue = 0,
                TotalGainLoss = 0,
                GainLossPercentage = 0,
                Positions = new List<PositionDetail>()
            };
        }

        var positionDetails = new List<PositionDetail>();
        decimal totalInvested = 0;
        decimal totalCurrentValue = 0;

        // Get current prices for all positions and convert to base currency
        foreach (var position in positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Symbol);
            
            // Convert invested amount to base currency
            var investedInOriginal = position.TotalInvested;
            var investedInBase = position.Currency == baseCurrency 
                ? investedInOriginal
                : await _currencyConverter.ConvertAsync(position.Currency, baseCurrency, investedInOriginal);
            
            // Convert current value to base currency
            var currentValueInOriginal = position.Quantity * currentPrice.Price;
            var currentValueInBase = currentPrice.Currency == baseCurrency
                ? currentValueInOriginal
                : await _currencyConverter.ConvertAsync(currentPrice.Currency, baseCurrency, currentValueInOriginal);
            
            var gainLoss = currentValueInBase - investedInBase;
            var gainLossPercent = investedInBase > 0 ? (gainLoss / investedInBase) * 100 : 0;

            positionDetails.Add(new PositionDetail
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AveragePurchasePrice = position.AveragePurchasePrice,
                CurrentPrice = currentPrice.Price,
                Currency = position.Currency,
                TotalInvested = investedInBase,
                CurrentValue = currentValueInBase,
                GainLoss = gainLoss,
                GainLossPercentage = gainLossPercent,
                FirstPurchaseDate = position.FirstPurchaseDate
            });

            totalInvested += investedInBase;
            totalCurrentValue += currentValueInBase;
        }

        var totalGainLoss = totalCurrentValue - totalInvested;
        var totalGainLossPercent = totalInvested > 0 ? (totalGainLoss / totalInvested) * 100 : 0;

        var summary = new PortfolioSummaryResponse
        {
            Username = username,
            BaseCurrency = baseCurrency,
            TotalInvested = totalInvested,
            CurrentValue = totalCurrentValue,
            TotalGainLoss = totalGainLoss,
            GainLossPercentage = totalGainLossPercent,
            Positions = positionDetails
        };

        _logger.LogInformation("Portfolio summary for {Username} in {BaseCurrency}: Invested={Invested}, Current={Current}, Gain/Loss={GainLoss} ({Percent}%)", 
            username, baseCurrency, totalInvested, totalCurrentValue, totalGainLoss, totalGainLossPercent);

        return summary;
    }

    /// <summary>
    /// Gets portfolio distribution for pie chart visualization
    /// Returns percentage breakdown of each stock's current value
    /// All values are converted to the specified base currency for accurate comparison
    /// </summary>
    public async Task<PortfolioDistributionResponse> GetPortfolioDistributionAsync(string username, string baseCurrency = "USD")
    {
        _logger.LogInformation("Generating portfolio distribution for {Username} in {BaseCurrency}", username, baseCurrency);

        var positions = await _repository.GetUserPositionsAsync(username);

        if (!positions.Any())
        {
            return new PortfolioDistributionResponse
            {
                Username = username,
                BaseCurrency = baseCurrency,
                TotalValue = 0,
                Holdings = new List<StockDistribution>()
            };
        }

        // Predefined color palette for pie chart (visually distinct colors)
        var colors = new[]
        {
            "#3B82F6", // Blue
            "#10B981", // Green
            "#F59E0B", // Amber
            "#EF4444", // Red
            "#8B5CF6", // Purple
            "#EC4899", // Pink
            "#14B8A6", // Teal
            "#F97316", // Orange
            "#6366F1", // Indigo
            "#84CC16"  // Lime
        };

        var holdings = new List<StockDistribution>();
        decimal totalPortfolioValue = 0;

        // First pass: calculate total portfolio value in base currency
        var positionValues = new List<(PortfolioPosition position, decimal currentPrice, string originalCurrency, decimal valueInBase)>();
        
        foreach (var position in positions)
        {
            var priceInfo = await _marketDataService.GetCurrentPriceAsync(position.Symbol);
            var currentValueInOriginal = position.Quantity * priceInfo.Price;
            
            // Convert to base currency
            var currentValueInBase = priceInfo.Currency == baseCurrency
                ? currentValueInOriginal
                : await _currencyConverter.ConvertAsync(priceInfo.Currency, baseCurrency, currentValueInOriginal);
            
            totalPortfolioValue += currentValueInBase;
            
            positionValues.Add((position, priceInfo.Price, priceInfo.Currency, currentValueInBase));
        }

        // Second pass: calculate percentages and assign colors
        var colorIndex = 0;
        foreach (var (position, currentPrice, originalCurrency, valueInBase) in positionValues.OrderByDescending(x => x.valueInBase))
        {
            var percentage = totalPortfolioValue > 0 ? (valueInBase / totalPortfolioValue) * 100 : 0;
            
            holdings.Add(new StockDistribution
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                Value = valueInBase,
                Percentage = Math.Round(percentage, 2),
                CurrentPrice = currentPrice,
                Currency = originalCurrency,
                OriginalCurrency = originalCurrency,
                Color = colors[colorIndex % colors.Length]
            });

            colorIndex++;
        }

        var response = new PortfolioDistributionResponse
        {
            Username = username,
            BaseCurrency = baseCurrency,
            TotalValue = totalPortfolioValue,
            Holdings = holdings
        };

        _logger.LogInformation("Portfolio distribution for {Username} in {BaseCurrency}: {Count} holdings, Total value={TotalValue}", 
            username, baseCurrency, holdings.Count, totalPortfolioValue);

        return response;
    }

    /// <summary>
    /// Checks if user has sufficient budget for a purchase
    /// </summary>
    public async Task<bool> CheckBudgetAsync(string username, decimal amount, string currency)
    {
        return await _budgetService.HasSufficientFundsAsync(username, amount, currency);
    }
}
