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
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(
        IPortfolioRepository repository, 
        IMarketDataService marketDataService,
        IBudgetService budgetService,
        ILogger<PortfolioService> logger)
    {
        _repository = repository;
        _marketDataService = marketDataService;
        _budgetService = budgetService;
        _logger = logger;
    }

    /// <summary>
    /// Executes a stock purchase
    /// 1. Gets current market price from MarketGateway
    /// 2. Checks user budget (placeholder for Budget service integration)
    /// 3. Updates or creates position with new average price
    /// 4. Records transaction
    /// </summary>
    public async Task<Transaction> BuyStockAsync(BuyStockRequest request)
    {
        _logger.LogInformation("Processing BUY order: {Username} buying {Quantity} shares of {Symbol}", 
            request.Username, request.Quantity, request.Symbol);

        // Get current market price
        var priceInfo = await _marketDataService.GetCurrentPriceAsync(request.Symbol);
        var totalCost = priceInfo.Price * request.Quantity;

        // TODO: Check and deduct from budget when Budget service is integrated
        // For now, this placeholder always returns true (no budget validation)
        var hasEnoughMoney = await _budgetService.DeductFromBudgetAsync(request.Username, totalCost);
        if (!hasEnoughMoney)
        {
            throw new Exception($"Insufficient funds. Required: ${totalCost:F2}");
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
            PricePerShare = priceInfo.Price
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
        _logger.LogInformation("Processing SELL order: {Username} selling {Quantity} shares of {Symbol}", 
            request.Username, request.Quantity, request.Symbol);

        // Check if user owns this stock
        var existingPosition = await _repository.GetPositionAsync(request.Username, request.Symbol);
        
        if (existingPosition == null)
        {
            throw new Exception($"User {request.Username} doesn't own any {request.Symbol} shares.");
        }

        if (existingPosition.Quantity < request.Quantity)
        {
            throw new Exception($"Insufficient shares. User owns {existingPosition.Quantity}, trying to sell {request.Quantity}.");
        }

        // Get current market price
        var priceInfo = await _marketDataService.GetCurrentPriceAsync(request.Symbol);
        var saleProceeds = priceInfo.Price * request.Quantity;

        // TODO: Add proceeds to budget when Budget service is integrated
        // For now, this placeholder does nothing
        await _budgetService.AddToBudgetAsync(request.Username, saleProceeds);

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
            PricePerShare = priceInfo.Price
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
    /// </summary>
    public async Task<PortfolioSummaryResponse> GetPortfolioSummaryAsync(string username)
    {
        _logger.LogInformation("Generating portfolio summary for {Username}", username);

        var positions = await _repository.GetUserPositionsAsync(username);

        if (!positions.Any())
        {
            return new PortfolioSummaryResponse
            {
                Username = username,
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

        // Get current prices for all positions
        foreach (var position in positions)
        {
            var currentPrice = await _marketDataService.GetCurrentPriceAsync(position.Symbol);
            
            var invested = position.TotalInvested;
            var currentValue = position.Quantity * currentPrice.Price;
            var gainLoss = currentValue - invested;
            var gainLossPercent = (gainLoss / invested) * 100;

            positionDetails.Add(new PositionDetail
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AveragePurchasePrice = position.AveragePurchasePrice,
                CurrentPrice = currentPrice.Price,
                TotalInvested = invested,
                CurrentValue = currentValue,
                GainLoss = gainLoss,
                GainLossPercentage = gainLossPercent,
                FirstPurchaseDate = position.FirstPurchaseDate
            });

            totalInvested += invested;
            totalCurrentValue += currentValue;
        }

        var totalGainLoss = totalCurrentValue - totalInvested;
        var totalGainLossPercent = totalInvested > 0 ? (totalGainLoss / totalInvested) * 100 : 0;

        var summary = new PortfolioSummaryResponse
        {
            Username = username,
            TotalInvested = totalInvested,
            CurrentValue = totalCurrentValue,
            TotalGainLoss = totalGainLoss,
            GainLossPercentage = totalGainLossPercent,
            Positions = positionDetails
        };

        _logger.LogInformation("Portfolio summary for {Username}: Invested=${Invested}, Current=${Current}, Gain/Loss=${GainLoss} ({Percent}%)", 
            username, totalInvested, totalCurrentValue, totalGainLoss, totalGainLossPercent);

        return summary;
    }

    /// <summary>
    /// Gets portfolio distribution for pie chart visualization
    /// Returns percentage breakdown of each stock's current value
    /// </summary>
    public async Task<PortfolioDistributionResponse> GetPortfolioDistributionAsync(string username)
    {
        _logger.LogInformation("Generating portfolio distribution for {Username}", username);

        var positions = await _repository.GetUserPositionsAsync(username);

        if (!positions.Any())
        {
            return new PortfolioDistributionResponse
            {
                Username = username,
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

        // First pass: calculate total portfolio value
        var positionValues = new List<(PortfolioPosition position, decimal currentPrice, decimal value)>();
        
        foreach (var position in positions)
        {
            var priceInfo = await _marketDataService.GetCurrentPriceAsync(position.Symbol);
            var currentValue = position.Quantity * priceInfo.Price;
            totalPortfolioValue += currentValue;
            
            positionValues.Add((position, priceInfo.Price, currentValue));
        }

        // Second pass: calculate percentages and assign colors
        var colorIndex = 0;
        foreach (var (position, currentPrice, value) in positionValues.OrderByDescending(x => x.value))
        {
            var percentage = totalPortfolioValue > 0 ? (value / totalPortfolioValue) * 100 : 0;
            
            holdings.Add(new StockDistribution
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                Value = value,
                Percentage = Math.Round(percentage, 2),
                CurrentPrice = currentPrice,
                Color = colors[colorIndex % colors.Length]
            });

            colorIndex++;
        }

        var response = new PortfolioDistributionResponse
        {
            Username = username,
            TotalValue = totalPortfolioValue,
            Holdings = holdings
        };

        _logger.LogInformation("Portfolio distribution for {Username}: {Count} holdings, Total value=${TotalValue}", 
            username, holdings.Count, totalPortfolioValue);

        return response;
    }

    /// <summary>
    /// Checks if user has sufficient budget for a purchase
    /// </summary>
    public async Task<bool> CheckBudgetAsync(string username, decimal amount)
    {
        return await _budgetService.HasSufficientFundsAsync(username, amount);
    }
}
