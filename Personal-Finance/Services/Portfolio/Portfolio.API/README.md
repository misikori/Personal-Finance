# 📊 Portfolio Service

A microservice for managing stock portfolios, executing buy/sell orders, tracking gains/losses, and predicting future stock prices.

---

## 🚀 Features

- ✅ **Buy Stocks** - Purchase stocks at current market prices
- ✅ **Sell Stocks** - Sell holdings and calculate gains/losses
- ✅ **Portfolio Summary** - View all positions with real-time values and performance metrics
- ✅ **Current Prices** - Get real-time stock prices from MarketGateway
- ✅ **Price Predictions** - AI-powered price predictions using historical data and trend analysis
- ✅ **Ready for Budget Integration** - Placeholder for Budget service integration

---

## 🏗️ Architecture

### Project Structure

```
Portfolio.API/          → REST API with Swagger UI
├── Controllers/        → API endpoints
├── Program.cs         → Service configuration
└── appsettings.json   → Configuration

Portfolio.Core/        → Business logic
├── Entities/         → Domain models (PortfolioPosition, Transaction)
├── DTOs/             → Request/Response models
├── Services/         → Business logic
│   ├── MarketDataService      → gRPC client for MarketGateway
│   ├── PortfolioService       → Main portfolio operations
│   └── PredictionService      → Price prediction algorithms
└── Repositories/     → Data access (in-memory for now)
```

### Communication

- **gRPC** → Communicates with `MarketGateway` for real-time and historical market data
- **REST** → Exposes HTTP API for frontend/clients
- **Future** → Will integrate with Budget service to check available funds

---

## 📋 Prerequisites

1. **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download)
2. **MarketGateway service** - Must be running on `http://localhost:5288`

---

## ⚙️ Configuration

Edit `appsettings.json`:

```json
{
  "Port": 5100,
  "MarketGatewayUrl": "http://localhost:5288"
}
```

- `Port` - Port the Portfolio API will run on (default: 5100)
- `MarketGatewayUrl` - URL of the MarketGateway gRPC service

---

## 🏃 How to Run

### Option 1: Command Line

```bash
# Navigate to the API directory
cd Personal-Finance/Services/Portfolio.API

# Run the service
dotnet run
```

The API will start at: **http://localhost:5100**

### Option 2: From Solution Root

```bash
# From Personal-Finance directory
dotnet run --project Services/Portfolio.API/Portfolio.API.csproj
```

### Option 3: Using Rider or Visual Studio

1. Open `Personal-Finance.sln`
2. Set `Portfolio.API` as the startup project
3. Press F5 or click Run

---

## 📚 API Documentation (Swagger)

Once the service is running, open your browser:

**Swagger UI**: http://localhost:5100

You'll see interactive API documentation where you can test all endpoints!

---

## 🔌 API Endpoints

### 1. Buy Stock

**POST** `/api/portfolio/buy`

Purchase stocks at current market price.

**Request Body:**
```json
{
  "username": "john_doe",
  "symbol": "AAPL",
  "quantity": 10
}
```

**Response:**
```json
{
  "id": "trans-123",
  "username": "john_doe",
  "symbol": "AAPL",
  "type": "BUY",
  "quantity": 10,
  "pricePerShare": 175.50,
  "totalValue": 1755.00,
  "transactionDate": "2025-10-01T14:30:00Z"
}
```

---

### 2. Sell Stock

**POST** `/api/portfolio/sell`

Sell stocks from your portfolio.

**Request Body:**
```json
{
  "username": "john_doe",
  "symbol": "AAPL",
  "quantity": 5
}
```

**Response:**
```json
{
  "id": "trans-456",
  "username": "john_doe",
  "symbol": "AAPL",
  "type": "SELL",
  "quantity": 5,
  "pricePerShare": 180.25,
  "totalValue": 901.25,
  "transactionDate": "2025-10-01T15:00:00Z"
}
```

---

### 3. Get Portfolio Summary

**GET** `/api/portfolio/summary/{username}`

Get complete portfolio with current values and gains/losses.

**Example:** `/api/portfolio/summary/john_doe`

**Response:**
```json
{
  "username": "john_doe",
  "totalInvested": 1755.00,
  "currentValue": 1802.50,
  "totalGainLoss": 47.50,
  "gainLossPercentage": 2.71,
  "positions": [
    {
      "symbol": "AAPL",
      "quantity": 5,
      "averagePurchasePrice": 175.50,
      "currentPrice": 180.50,
      "totalInvested": 877.50,
      "currentValue": 902.50,
      "gainLoss": 25.00,
      "gainLossPercentage": 2.85,
      "firstPurchaseDate": "2025-09-15T10:00:00Z"
    }
  ]
}
```

---

### 4. Get Current Stock Price

**GET** `/api/portfolio/price/{symbol}`

Fetch real-time stock price from MarketGateway.

**Example:** `/api/portfolio/price/TSLA`

**Response:**
```json
{
  "symbol": "TSLA",
  "price": 255.75,
  "open": 252.00,
  "high": 258.50,
  "low": 251.30,
  "previousClose": 253.20,
  "volume": 12500000,
  "asOf": "2025-10-01T16:00:00Z"
}
```

---

### 5. Get Price Prediction

**GET** `/api/portfolio/predict/{symbol}`

Get AI-powered price prediction based on historical data.

**Example:** `/api/portfolio/predict/AAPL`

**Response:**
```json
{
  "symbol": "AAPL",
  "currentPrice": 175.50,
  "predictedPrice": 178.25,
  "predictedChangePercent": 1.57,
  "confidence": 72.5,
  "method": "Simple Moving Average (SMA) with Trend Analysis",
  "generatedAt": "2025-10-01T16:30:00Z"
}
```

---

### 6. Health Check

**GET** `/api/portfolio/health`

Check if the service is running.

**Response:**
```json
{
  "status": "healthy",
  "service": "Portfolio.API",
  "timestamp": "2025-10-01T16:45:00Z"
}
```

---

## 🧪 Testing the API

### Using Swagger UI (Recommended)

1. Open http://localhost:5100
2. Expand any endpoint
3. Click "Try it out"
4. Fill in the parameters
5. Click "Execute"

### Using curl

```bash
# Buy stock
curl -X POST http://localhost:5100/api/portfolio/buy \
  -H "Content-Type: application/json" \
  -d '{"username":"john_doe","symbol":"AAPL","quantity":10}'

# Get portfolio summary
curl http://localhost:5100/api/portfolio/summary/john_doe

# Get current price
curl http://localhost:5100/api/portfolio/price/TSLA

# Get price prediction
curl http://localhost:5100/api/portfolio/predict/AAPL
```

### Using HTTP Client (Rider/VS Code)

Create a `.http` file:

```http
### Buy Stock
POST http://localhost:5100/api/portfolio/buy
Content-Type: application/json

{
  "username": "john_doe",
  "symbol": "AAPL",
  "quantity": 10
}

### Get Portfolio Summary
GET http://localhost:5100/api/portfolio/summary/john_doe

### Get Current Price
GET http://localhost:5100/api/portfolio/price/AAPL

### Get Price Prediction
GET http://localhost:5100/api/portfolio/predict/AAPL
```

---

## 🔮 Price Prediction Algorithm

The prediction service uses a **Simple Moving Average (SMA)** strategy:

1. **Fetches 30 days** of historical price data from MarketGateway
2. **Calculates SMAs** for 5, 10, and 20-day periods
3. **Analyzes trend** - if short-term SMA > long-term SMA = uptrend
4. **Predicts next price** based on current trend momentum
5. **Calculates confidence** based on price volatility (standard deviation)

**Note**: This is a basic implementation. In production, you'd use:
- Machine learning models (LSTM, Prophet, etc.)
- Technical indicators (RSI, MACD, Bollinger Bands)
- Fundamental analysis
- Sentiment analysis

---

## 💾 Data Storage

Currently uses **in-memory storage** for:
- Portfolio positions
- Transaction history

**For production**, replace with:
- SQL Server / PostgreSQL (Entity Framework Core)
- MongoDB (document store)
- Redis (caching layer)

Update `PortfolioRepository.cs` to use a real database.

---

## 🔗 Budget Service Integration

The service is ready to integrate with a Budget microservice:

**Current behavior** (line 254 in `PortfolioService.cs`):
```csharp
public Task<bool> CheckBudgetAsync(string username, decimal amount)
{
    // TODO: Call Budget service via gRPC/REST
    // For now: always returns true (unlimited budget)
    return Task.FromResult(true);
}
```

**Future implementation**:
```csharp
var budgetClient = new BudgetService.BudgetServiceClient(budgetChannel);
var response = await budgetClient.CheckAvailableFundsAsync(
    new CheckFundsRequest { Username = username, Amount = amount }
);
return response.HasSufficientFunds;
```

---

## 🐛 Troubleshooting

### "Failed to connect to MarketGateway"

**Solution**: Make sure MarketGateway is running:
```bash
cd MarketGateway/MarketGateway.Core
dotnet run
```

### "Insufficient historical data for predictions"

**Solution**: The stock symbol might not have enough historical data in MarketGateway. Try symbols like `AAPL`, `TSLA`, `MSFT`.

### Port already in use

**Solution**: Change the port in `appsettings.json`:
```json
{
  "Port": 5101
}
```

---

## 📊 Example Workflow

```bash
# 1. Start MarketGateway (Terminal 1)
cd MarketGateway/MarketGateway.Core
dotnet run

# 2. Start Portfolio API (Terminal 2)
cd Personal-Finance/Services/Portfolio.API
dotnet run

# 3. Open Swagger UI
# Browser: http://localhost:5100

# 4. Buy some stocks
POST /api/portfolio/buy
{
  "username": "alice",
  "symbol": "AAPL",
  "quantity": 10
}

# 5. Check portfolio
GET /api/portfolio/summary/alice

# 6. Get price prediction
GET /api/portfolio/predict/AAPL

# 7. Sell if prediction is negative
POST /api/portfolio/sell
{
  "username": "alice",
  "symbol": "AAPL",
  "quantity": 5
}
```

---

## 🚀 Next Steps

1. **Add Database** - Replace in-memory storage with SQL/PostgreSQL
2. **Integrate Budget Service** - Implement real budget checks
3. **Add Authentication** - JWT tokens, user roles
4. **Real-time Updates** - SignalR for live portfolio updates
5. **Advanced Predictions** - ML models, technical indicators
6. **Notifications** - Email/SMS alerts for price targets
7. **Docker** - Containerize the service
8. **Tests** - Unit tests, integration tests

---

## 📝 API Reference

Full API documentation is available at the Swagger UI when the service is running:

👉 **http://localhost:5100**

---

## 🤝 Dependencies

- **MarketGateway.Contracts** - gRPC contracts for market data
- **Grpc.Net.Client** - gRPC client library
- **Google.Protobuf** - Protocol Buffers support
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation

---

## 📧 Support

For issues or questions:
1. Check the Swagger UI documentation
2. Review logs in the console output
3. Ensure MarketGateway is running and accessible

---

**Happy Trading! 📈**


