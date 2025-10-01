# 🏦 Portfolio Management Service

A microservice for stock portfolio management with **placeholder integration** for Budget service.

---

## ⚠️ IMPORTANT: Budget Service Status

**The Budget functionality is currently a PLACEHOLDER.**

- ✅ Portfolio service is fully functional for stock tracking
- ⚠️ Budget checks are **not enforced** (placeholder always returns `true`)
- 📝 Budget service integration is ready but waiting for your Budget microservice

See **[BUDGET_INTEGRATION.md](./BUDGET_INTEGRATION.md)** for complete details on how to integrate.

---

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK installed
- SQL Server running (can use Docker or local instance)

### Option 1: Run Existing Services + Portfolio API

```bash
# Step 1: Start existing services with Docker Compose
cd /Users/mradosavljevic/Desktop/Personal-Finance/Personal-Finance
docker-compose up -d

# This starts:
# - MSSQL Server (localhost:1433)
# - IdentityServer (localhost:8003)
# - Currency API (localhost:8001)
# - Currency gRPC (localhost:8002)

# Step 2: Run Portfolio API locally
cd Services/Portfolio/Portfolio.API
dotnet run

# Portfolio API will start on: http://localhost:5100
# Swagger UI: http://localhost:5100/swagger
```

### Option 2: Run Everything Locally (No Docker)

```bash
# Terminal 1: Start SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=MATF12345678rs2" \
  -p 1433:1433 --name mssql \
  mcr.microsoft.com/mssql/server:2022-latest

# Terminal 2: IdentityServer
cd Security/IdentityServer
dotnet run
# Runs on: http://localhost:5000

# Terminal 3: Portfolio API
cd Services/Portfolio/Portfolio.API
dotnet run
# Runs on: http://localhost:5100
```

---

## ✨ Features

### Currently Working
- ✅ **Buy/Sell Stocks** - Real-time market prices via MarketGateway
- ✅ **Portfolio Tracking** - View positions with gains/losses  
- ✅ **Transaction History** - Complete audit trail
- ✅ **Price Predictions** - SMA-based predictions with confidence scores
- ✅ **SQL Server Database** - Positions and Transactions tables
- ✅ **JWT Authentication** - Integrated with IdentityServer

### Placeholder (Waiting for Budget Service)
- ⚠️ **Budget Validation** - Currently allows all purchases (no budget check)
- ⚠️ **Money Tracking** - Selling stocks doesn't add to budget
- 📝 Ready for integration - see [BUDGET_INTEGRATION.md](./BUDGET_INTEGRATION.md)

---

## 📁 Project Structure

```
Portfolio/
├── Portfolio.API/              # REST API Layer
│   ├── Controllers/
│   │   └── PortfolioController.cs   # Buy/Sell/Summary endpoints
│   ├── Program.cs              # Service configuration
│   └── appsettings.json        # Configuration
│
├── Portfolio.Core/             # Business Logic Layer
│   ├── Services/
│   │   ├── PortfolioService.cs         # Buy/sell logic
│   │   ├── BudgetServicePlaceholder.cs  # ⚠️ PLACEHOLDER
│   │   ├── IBudgetService.cs            # Interface for Budget integration
│   │   ├── MarketDataService.cs         # gRPC client for prices
│   │   └── PredictionService.cs         # Price predictions
│   ├── Entities/
│   │   ├── PortfolioPosition.cs         # Domain models
│   │   └── Transaction.cs
│   └── DTOs/                   # Request/Response models
│
└── Portfolio.Data/             # Data Access Layer
    ├── PortfolioDbContext.cs   # EF Core context
    ├── Repositories/
    │   └── PortfolioRepository.cs   # SQL Server data access
    └── Migrations/             # EF Core migrations
```

---

## 📖 API Endpoints

### Portfolio Operations
| Endpoint | Method | Description | Budget Integration |
|----------|--------|-------------|-------------------|
| `/api/portfolio/buy` | POST | Buy stocks | ⚠️ Placeholder (no actual deduction) |
| `/api/portfolio/sell` | POST | Sell stocks | ⚠️ Placeholder (no actual addition) |
| `/api/portfolio/summary/{username}` | GET | Portfolio with gains/losses | ✅ Works |
| `/api/portfolio/price/{symbol}` | GET | Current stock price | ✅ Works |
| `/api/portfolio/predict/{symbol}` | GET | Price prediction | ✅ Works |

---

## 🧪 Testing the API

### 1. Register User (IdentityServer)
```bash
curl -X POST http://localhost:8003/api/Authentication/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "Password123!"
  }'
```

### 2. Login and Get JWT Token
```bash
curl -X POST http://localhost:8003/api/Authentication/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Password123!"
  }'

# Save the token from response!
```

### 3. Buy Stock
```bash
curl -X POST http://localhost:5100/api/Portfolio/buy \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "symbol": "AAPL",
    "quantity": 10
  }'
```

### 4. View Portfolio
```bash
curl -X GET http://localhost:5100/api/Portfolio/summary/testuser \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### 5. Sell Stock
```bash
curl -X POST http://localhost:5100/api/Portfolio/sell \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "symbol": "AAPL",
    "quantity": 5
  }'
```

---

## 🗄️ Database Schema

### Positions Table
Stores user's stock holdings
```sql
- Id: Unique identifier
- Username: User who owns the position
- Symbol: Stock symbol (e.g., AAPL)
- Quantity: Number of shares
- AveragePurchasePrice: Average cost basis
- FirstPurchaseDate: When first purchased
```

### Transactions Table
Complete buy/sell history
```sql
- Id: Unique identifier
- Username: User who made transaction
- Symbol: Stock symbol
- Type: "BUY" or "SELL"
- Quantity: Number of shares
- PricePerShare: Execution price
- TransactionDate: When executed
```

### ~~UserBudgets Table~~ ❌ REMOVED
Budget data is managed by the separate **Budget microservice**.

---

## 🔧 Configuration

### Connection Strings

**appsettings.json (Local):**
```json
{
  "ConnectionStrings": {
    "PortfolioDb": "Server=localhost;Database=PortfolioDb;User Id=sa;Password=MATF12345678rs2;TrustServerCertificate=True;"
  }
}
```

**appsettings.Development.json (Docker):**
```json
{
  "ConnectionStrings": {
    "PortfolioDb": "Server=mssql;Database=PortfolioDb;User Id=sa;Password=MATF12345678rs2;TrustServerCertificate=True;"
  }
}
```

### JWT Settings
Must match IdentityServer settings:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration123!MustBeLongEnough",
    "Issuer": "IdentityServer",
    "Audience": "PortfolioAPI",
    "ExpirationMinutes": 60
  }
}
```

---

## 🔍 Troubleshooting

### "All purchases succeed even with $0 balance"
**Expected Behavior:** This is normal - Budget service is not integrated yet.  
**Solution:** Integrate your Budget service (see [BUDGET_INTEGRATION.md](./BUDGET_INTEGRATION.md))

### Database Connection Errors
```bash
# Make sure SQL Server is running
docker ps | grep mssql

# If not running, start it
docker start mssql
# Or start with docker-compose
cd ../../.. && docker-compose up -d mssql
```

### "Unauthorized" (401) Errors
- Make sure you logged in and got a JWT token
- Include token in Authorization header: `Bearer YOUR_TOKEN`
- Token expires after 60 minutes - login again

### Check Placeholder Logs
```bash
# When running Portfolio API, you'll see warnings like:
[Warning] BudgetService not integrated - DeductFromBudgetAsync does nothing
[Warning] BudgetService not integrated - AddToBudgetAsync does nothing
```

This is expected - the Budget service is a placeholder!

---

## 📚 Documentation

- **[BUDGET_INTEGRATION.md](./BUDGET_INTEGRATION.md)** - ⭐ How to integrate your Budget service
- **[CHANGES_SUMMARY.md](./CHANGES_SUMMARY.md)** - What was changed (budget removed)

---

## 🧰 Technology Stack

- **Framework**: .NET 8.0
- **Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT Bearer
- **API**: ASP.NET Core Web API
- **Documentation**: Swagger/OpenAPI
- **gRPC**: Grpc.Net.Client (for MarketGateway)

---

## 🎯 Service Dependencies

```
Portfolio API depends on:
├── IdentityServer (localhost:8003) - JWT token generation
├── MSSQL (localhost:1433) - Database
└── MarketGateway (localhost:5288) - Stock prices [OPTIONAL]
```

**Note:** MarketGateway is optional. If not running, price fetching will fail but the service will still work.

---

## 📝 Database Migrations

Migrations are automatically applied on startup. To manually manage migrations:

```bash
cd Portfolio.API

# Create new migration
dotnet ef migrations add MigrationName \
  --project ../Portfolio.Data/Portfolio.Data.csproj \
  --context PortfolioDbContext

# Apply migrations manually
dotnet ef database update \
  --project ../Portfolio.Data/Portfolio.Data.csproj \
  --context PortfolioDbContext

# Remove last migration
dotnet ef migrations remove \
  --project ../Portfolio.Data/Portfolio.Data.csproj \
  --context PortfolioDbContext
```

---

## 🎉 Summary

**What Works:**
- ✅ Stock portfolio tracking
- ✅ Buy/sell operations (records transactions)
- ✅ Gains/loss calculations
- ✅ Price predictions
- ✅ SQL Server persistence
- ✅ JWT authentication

**What's Placeholder:**
- ⚠️ Budget validation (always succeeds)
- ⚠️ Money deduction/addition (does nothing)

**Ready for Integration:**
- ✅ Interface defined (`IBudgetService`)
- ✅ Placeholder implemented
- ✅ TODO comments in place
- ✅ Integration guide provided

---

**Start trading stocks! 📈** 

*(Budget tracking coming soon when your Budget service is ready!)*
