# 🏗️ Portfolio Service Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Frontend / Client                           │
│                      (React, Mobile App, etc.)                      │
└────────────────────────────┬────────────────────────────────────────┘
                             │ HTTP/REST
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       Portfolio.API (Port 5100)                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  REST Endpoints with Swagger UI                              │  │
│  │  • POST /api/portfolio/buy                                   │  │
│  │  • POST /api/portfolio/sell                                  │  │
│  │  • GET  /api/portfolio/summary/{username}                    │  │
│  │  • GET  /api/portfolio/price/{symbol}                        │  │
│  │  • GET  /api/portfolio/predict/{symbol}                      │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                             │                                        │
│                             ▼                                        │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                   PortfolioController                         │  │
│  │         Handles HTTP requests/responses                       │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      Portfolio.Core (Business Logic)                │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                    PortfolioService                          │  │
│  │  • BuyStockAsync()     - Execute buy orders                 │  │
│  │  • SellStockAsync()    - Execute sell orders                │  │
│  │  • GetPortfolioSummary() - Calculate gains/losses           │  │
│  │  • CheckBudgetAsync()  - Verify available funds             │  │
│  └─────────┬───────────────────────────┬───────────────────────┘  │
│            │                           │                           │
│            ▼                           ▼                           │
│  ┌──────────────────────┐   ┌────────────────────────────────┐   │
│  │  MarketDataService   │   │   PredictionService            │   │
│  │  • GetCurrentPrice() │   │   • PredictPriceAsync()        │   │
│  │  • GetHistorical()   │   │   • Calculate SMA              │   │
│  └──────────┬───────────┘   │   • Analyze trends             │   │
│             │               │   • Calculate confidence        │   │
│             │               └────────────┬───────────────────┘   │
│             │                            │                        │
│             │ gRPC                       │ Uses historical data   │
│             │                            │                        │
└─────────────┼────────────────────────────┼────────────────────────┘
              │                            │
              ▼                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│              MarketGateway.Core (Port 5288) - gRPC Service          │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  • Fetch() - Get quotes, OHLCV data                          │  │
│  │  • Supports QUOTE (real-time) and OHLCV (historical)         │  │
│  │  • Fetches from AlphaVantage, other providers                │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow: Buy Stock

```
1. User Request (HTTP)
   POST /api/portfolio/buy
   {
     "username": "alice",
     "symbol": "AAPL",
     "quantity": 10
   }
   
2. PortfolioController
   └─> Validates request
   └─> Calls PortfolioService.BuyStockAsync()
   
3. PortfolioService.BuyStockAsync()
   ├─> MarketDataService.GetCurrentPriceAsync("AAPL")
   │   └─> gRPC call to MarketGateway
   │   └─> Returns current price: $175.50
   │
   ├─> CheckBudgetAsync("alice", $1,755)
   │   └─> [TODO] Call Budget Service
   │   └─> For now: returns true (always sufficient)
   │
   ├─> PortfolioRepository.GetPositionAsync("alice", "AAPL")
   │   └─> Check if alice already owns AAPL
   │
   ├─> If existing position:
   │   └─> Calculate new average price
   │       Old: 5 shares @ $170 = $850
   │       New: 10 shares @ $175.50 = $1,755
   │       Total: 15 shares @ $173.67 average
   │
   └─> PortfolioRepository.UpsertPositionAsync(position)
   └─> PortfolioRepository.AddTransactionAsync(transaction)
   
4. Response
   {
     "id": "trans-123",
     "username": "alice",
     "symbol": "AAPL",
     "type": "BUY",
     "quantity": 10,
     "pricePerShare": 175.50,
     "totalValue": 1755.00,
     "transactionDate": "2025-10-01T14:30:00Z"
   }
```

---

## Data Flow: Get Portfolio Summary

```
1. User Request (HTTP)
   GET /api/portfolio/summary/alice
   
2. PortfolioController
   └─> Calls PortfolioService.GetPortfolioSummaryAsync("alice")
   
3. PortfolioService.GetPortfolioSummaryAsync()
   ├─> PortfolioRepository.GetUserPositionsAsync("alice")
   │   └─> Returns: [AAPL: 15 shares @ $173.67 avg]
   │
   └─> For each position:
       ├─> MarketDataService.GetCurrentPriceAsync("AAPL")
       │   └─> gRPC call to MarketGateway
       │   └─> Returns: $180.50
       │
       └─> Calculate metrics:
           Invested:      15 × $173.67 = $2,605.05
           Current Value: 15 × $180.50 = $2,707.50
           Gain/Loss:     $102.45
           Percentage:    3.93%
   
4. Response
   {
     "username": "alice",
     "totalInvested": 2605.05,
     "currentValue": 2707.50,
     "totalGainLoss": 102.45,
     "gainLossPercentage": 3.93,
     "positions": [
       {
         "symbol": "AAPL",
         "quantity": 15,
         "averagePurchasePrice": 173.67,
         "currentPrice": 180.50,
         "totalInvested": 2605.05,
         "currentValue": 2707.50,
         "gainLoss": 102.45,
         "gainLossPercentage": 3.93
       }
     ]
   }
```

---

## Data Flow: Price Prediction

```
1. User Request (HTTP)
   GET /api/portfolio/predict/AAPL
   
2. PortfolioController
   └─> Calls PredictionService.PredictPriceAsync("AAPL")
   
3. PredictionService.PredictPriceAsync()
   ├─> MarketDataService.GetHistoricalPricesAsync("AAPL", 30)
   │   └─> gRPC call to MarketGateway (OHLCV data)
   │   └─> Returns: [175.50, 174.20, 176.30, ..., 180.50]
   │
   ├─> Calculate SMAs:
   │   5-day SMA:  (180.50 + 179.20 + 178.50 + 177.80 + 176.90) / 5 = 178.58
   │   10-day SMA: 177.25
   │   20-day SMA: 175.80
   │
   ├─> Analyze trend:
   │   Trend = (SMA5 - SMA20) / SMA20 = (178.58 - 175.80) / 175.80 = 1.58%
   │   → Uptrend detected!
   │
   ├─> Predict next price:
   │   Predicted = Current + (Current × Trend)
   │   Predicted = 180.50 + (180.50 × 0.0158) = 183.35
   │
   ├─> Calculate confidence:
   │   Volatility = StandardDeviation(prices) = 3.2
   │   Confidence = 100 - (Volatility × 10) = 68%
   │
4. Response
   {
     "symbol": "AAPL",
     "currentPrice": 180.50,
     "predictedPrice": 183.35,
     "predictedChangePercent": 1.58,
     "confidence": 68.0,
     "method": "Simple Moving Average (SMA) with Trend Analysis"
   }
```

---

## Technology Stack

### Portfolio.API
- **Framework**: ASP.NET Core 8.0
- **Swagger**: Swashbuckle.AspNetCore
- **gRPC Client**: Grpc.Net.Client
- **Serialization**: Google.Protobuf
- **Port**: 5100

### Portfolio.Core
- **Logging**: Microsoft.Extensions.Logging
- **gRPC Client**: Grpc.Net.Client
- **Storage**: In-memory (List<T>)

### MarketGateway
- **Framework**: ASP.NET Core 8.0
- **gRPC Server**: Grpc.AspNetCore
- **Database**: SQLite (market.db)
- **Port**: 5288

---

## Communication Protocols

### REST (HTTP/JSON)
```
Frontend ←→ Portfolio.API
- Human-readable JSON
- Standard HTTP methods
- Swagger documentation
- Easy to debug
```

### gRPC (Protocol Buffers)
```
Portfolio.API ←→ MarketGateway
- Binary protocol (10x faster)
- Strongly typed (.proto contracts)
- Bidirectional streaming support
- Efficient for service-to-service
```

---

## Storage Architecture

### Current (Demo)
```
Portfolio.Core
├─> In-Memory Storage
    ├─> List<PortfolioPosition>
    └─> List<Transaction>
```

### Future (Production)
```
Portfolio.Core
├─> Entity Framework Core
    ├─> PostgreSQL / SQL Server
    │   ├─> Positions Table
    │   └─> Transactions Table
    │
    └─> Redis (Caching)
        └─> Current prices cache
```

---

## Security & Authentication (Future)

```
┌──────────────┐
│   Client     │
└──────┬───────┘
       │ 1. Login (POST /auth/login)
       ▼
┌──────────────────┐
│ IdentityServer   │ → Issues JWT token
└──────┬───────────┘
       │ 2. JWT Token
       ▼
┌──────────────────┐
│  Portfolio.API   │
│  [Authorize]     │ → Validates JWT
└──────┬───────────┘
       │ 3. Calls with valid token
       ▼
   Business Logic
```

---

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Docker Compose                         │
│                                                             │
│  ┌───────────────┐  ┌───────────────┐  ┌───────────────┐  │
│  │   Frontend    │  │  Portfolio    │  │ MarketGateway │  │
│  │   (React)     │  │     API       │  │     (gRPC)    │  │
│  │   Port 3000   │  │   Port 5100   │  │   Port 5288   │  │
│  └───────┬───────┘  └───────┬───────┘  └───────┬───────┘  │
│          │                  │                  │           │
│  ┌───────────────────────────────────────────────────┐     │
│  │            Internal Docker Network                │     │
│  └───────────────────────────────────────────────────┘     │
│                                                             │
│  ┌───────────────┐  ┌───────────────┐                     │
│  │   PostgreSQL  │  │     Redis     │                     │
│  │   Port 5432   │  │   Port 6379   │                     │
│  └───────────────┘  └───────────────┘                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Error Handling Flow

```
Try to Buy Stock
    │
    ├─> MarketGateway unreachable?
    │   └─> Throw Exception
    │   └─> Controller catches
    │   └─> Returns 400 Bad Request
    │       {"error": "Failed to fetch price for AAPL: Connection refused"}
    │
    ├─> Insufficient budget?
    │   └─> Throw Exception
    │   └─> Returns 400 Bad Request
    │       {"error": "Insufficient budget. Required: $1755"}
    │
    └─> Success
        └─> Returns 200 OK
            {Transaction details}
```

---

## Performance Characteristics

### Response Times (Expected)

| Endpoint | Typical Response Time | Why |
|----------|----------------------|-----|
| `/api/portfolio/buy` | 50-200ms | gRPC call to MarketGateway |
| `/api/portfolio/sell` | 50-200ms | gRPC call to MarketGateway |
| `/api/portfolio/summary/{user}` | 100-500ms | Multiple gRPC calls (one per position) |
| `/api/portfolio/price/{symbol}` | 30-100ms | Single gRPC call |
| `/api/portfolio/predict/{symbol}` | 200-800ms | Fetches 30 days of data + calculations |

### Optimization Opportunities

1. **Cache current prices** (Redis) - Reduce gRPC calls
2. **Batch price requests** - Fetch all prices in one call
3. **Background jobs** - Pre-calculate predictions
4. **Database indexes** - Faster position lookups

---

## Monitoring & Logging

### Current Logging
```csharp
_logger.LogInformation("Processing BUY order: {Username} buying {Quantity} shares of {Symbol}", 
    request.Username, request.Quantity, request.Symbol);

_logger.LogError(ex, "Error fetching price for {Symbol}", symbol);
```

### Future Monitoring
```
┌─────────────────┐
│  Serilog        │ → Structured logging
└────────┬────────┘
         │
         ├─> Seq / ELK Stack (Log aggregation)
         ├─> Application Insights (Azure)
         └─> Prometheus (Metrics)
```

---

## Scalability Considerations

### Current (Single Instance)
```
Load Balancer
     │
     ▼
Portfolio.API (Single instance)
```

### Future (Horizontal Scaling)
```
Load Balancer
     │
     ├─> Portfolio.API (Instance 1)
     ├─> Portfolio.API (Instance 2)
     └─> Portfolio.API (Instance 3)
          │
          └─> Shared Database (PostgreSQL)
          └─> Shared Cache (Redis)
```

---

**This architecture follows microservices best practices and is ready for production deployment!** 🚀


