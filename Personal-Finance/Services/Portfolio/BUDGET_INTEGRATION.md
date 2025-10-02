# 💰 Budget Service Integration - Placeholder Status

## ⚠️ Current Status: PLACEHOLDER IMPLEMENTATION

The Portfolio service currently has **placeholder methods** for Budget service integration. These placeholders **do nothing** and are ready for you to connect to your actual Budget microservice when it's ready.

---

## 🔍 What's Implemented

### IBudgetService Interface
Located: `Portfolio.Core/Services/IBudgetService.cs`

```csharp
public interface IBudgetService
{
    Task<bool> HasSufficientFundsAsync(string username, decimal amount);
    Task<bool> DeductFromBudgetAsync(string username, decimal amount);
    Task<bool> AddToBudgetAsync(string username, decimal amount);
}
```

### BudgetServicePlaceholder Class
Located: `Portfolio.Core/Services/BudgetServicePlaceholder.cs`

**Current Behavior:**
- ✅ All methods return `true` (success)
- ⚠️ No actual budget checks or modifications
- 📝 Logs warnings when called

```csharp
public class BudgetServicePlaceholder : IBudgetService
{
    // Always returns true - no actual validation
    public Task<bool> HasSufficientFundsAsync(string username, decimal amount)
    {
        _logger.LogWarning("BudgetService not integrated - always returns true");
        return Task.FromResult(true);
    }

    // Always returns true - doesn't actually deduct
    public Task<bool> DeductFromBudgetAsync(string username, decimal amount)
    {
        _logger.LogWarning("BudgetService not integrated - does nothing");
        return Task.FromResult(true);
    }

    // Always returns true - doesn't actually add
    public Task<bool> AddToBudgetAsync(string username, decimal amount)
    {
        _logger.LogWarning("BudgetService not integrated - does nothing");
        return Task.FromResult(true);
    }
}
```

---

## 🔗 Where Budget Service is Called

### 1. **BuyStockAsync** (PortfolioService.cs)
```csharp
// Line 47-53
// TODO: Check and deduct from budget when Budget service is integrated
// For now, this placeholder always returns true (no budget validation)
var hasEnoughMoney = await _budgetService.DeductFromBudgetAsync(request.Username, totalCost);
if (!hasEnoughMoney)
{
    throw new Exception($"Insufficient funds. Required: ${totalCost:F2}");
}
```

**Current Behavior:** Always allows purchase (no budget check)

---

### 2. **SellStockAsync** (PortfolioService.cs)
```csharp
// Line 141-143
// TODO: Add proceeds to budget when Budget service is integrated
// For now, this placeholder does nothing
await _budgetService.AddToBudgetAsync(request.Username, saleProceeds);
```

**Current Behavior:** Doesn't add money to budget

---

## 🛠️ How to Integrate Your Budget Service

When your Budget service is ready, follow these steps:

### Step 1: Choose Integration Method

**Option A: gRPC (Recommended for microservices)**
```csharp
// Create Budget.Contracts project with .proto files
syntax = "proto3";

service BudgetService {
  rpc CheckFunds(CheckFundsRequest) returns (CheckFundsResponse);
  rpc DeductFunds(DeductFundsRequest) returns (DeductFundsResponse);
  rpc AddFunds(AddFundsRequest) returns (AddFundsResponse);
}
```

**Option B: REST API**
```csharp
// HTTP client calling Budget API
var response = await _httpClient.GetAsync($"http://budget-service/api/budget/{username}/check?amount={amount}");
```

### Step 2: Create Budget Service Client

Create a new class: `Portfolio.Core/Services/BudgetServiceClient.cs`

```csharp
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Portfolio.Core.Services;

public class BudgetServiceClient : IBudgetService
{
    private readonly GrpcChannel _channel;
    private readonly ILogger<BudgetServiceClient> _logger;

    public BudgetServiceClient(string budgetServiceUrl, ILogger<BudgetServiceClient> logger)
    {
        _channel = GrpcChannel.ForAddress(budgetServiceUrl);
        _logger = logger;
    }

    public async Task<bool> HasSufficientFundsAsync(string username, decimal amount)
    {
        try
        {
            var client = new BudgetService.BudgetServiceClient(_channel);
            var response = await client.CheckFundsAsync(new CheckFundsRequest 
            { 
                Username = username, 
                Amount = (double)amount 
            });
            return response.HasSufficientFunds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking funds for {Username}", username);
            throw;
        }
    }

    public async Task<bool> DeductFromBudgetAsync(string username, decimal amount)
    {
        try
        {
            var client = new BudgetService.BudgetServiceClient(_channel);
            var response = await client.DeductFundsAsync(new DeductFundsRequest 
            { 
                Username = username, 
                Amount = (double)amount 
            });
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deducting funds for {Username}", username);
            throw;
        }
    }

    public async Task<bool> AddToBudgetAsync(string username, decimal amount)
    {
        try
        {
            var client = new BudgetService.BudgetServiceClient(_channel);
            var response = await client.AddFundsAsync(new AddFundsRequest 
            { 
                Username = username, 
                Amount = (double)amount 
            });
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding funds for {Username}", username);
            throw;
        }
    }
}
```

### Step 3: Update Program.cs

Replace the placeholder with your client:

```csharp
// In Portfolio.API/Program.cs

// OLD (placeholder)
builder.Services.AddScoped<IBudgetService, BudgetServicePlaceholder>();

// NEW (real implementation)
var budgetServiceUrl = builder.Configuration.GetValue<string>("BudgetServiceUrl") 
    ?? "http://localhost:5200";
builder.Services.AddScoped<IBudgetService>(sp => 
    new BudgetServiceClient(
        budgetServiceUrl,
        sp.GetRequiredService<ILogger<BudgetServiceClient>>()
    ));
```

### Step 4: Add Configuration

In `appsettings.json`:
```json
{
  "BudgetServiceUrl": "http://localhost:5200",
  ...
}
```

In `appsettings.Development.json` (Docker):
```json
{
  "BudgetServiceUrl": "http://budget-service:5200",
  ...
}
```

### Step 5: Update docker-compose.yaml

```yaml
services:
  budget.service:
    build:
      context: .
      dockerfile: Services/Budget/Budget.API/Dockerfile
    ports:
      - "8006:8080"
  
  portfolio.api:
    depends_on:
      - budget.service
    environment:
      - "BudgetServiceUrl=http://budget.service:8080"
```

### Step 6: Delete Placeholder

Once your Budget service is integrated:
```bash
rm Portfolio.Core/Services/BudgetServicePlaceholder.cs
```

---

## ✅ Testing Integration

### Before Integration (Current)
```bash
# Buy stock - always succeeds (no budget check)
curl -X POST http://localhost:8004/api/Portfolio/buy \
  -H "Authorization: Bearer TOKEN" \
  -d '{"username":"john","symbol":"AAPL","quantity":999999}'

# Response: Success (even though user might not have funds)
```

### After Integration
```bash
# Buy stock - budget is validated
curl -X POST http://localhost:8004/api/Portfolio/buy \
  -H "Authorization: Bearer TOKEN" \
  -d '{"username":"john","symbol":"AAPL","quantity":999999}'

# Response: "Insufficient funds" (if user doesn't have enough money)
```

---

## 📝 Budget Service Requirements

Your Budget service should provide these capabilities:

### 1. Check Funds
**Purpose:** Verify user has enough money  
**Input:** `username`, `amount`  
**Output:** `bool hasSufficientFunds`

### 2. Deduct Funds
**Purpose:** Remove money when buying stocks  
**Input:** `username`, `amount`  
**Output:** `bool success`  
**Behavior:** Return `false` if insufficient funds

### 3. Add Funds
**Purpose:** Add money when selling stocks  
**Input:** `username`, `amount`  
**Output:** `bool success`

---

## 🔐 Security Considerations

When integrating Budget service:

1. **Authentication**
   - Portfolio service should authenticate with Budget service
   - Consider service-to-service authentication (JWT, mTLS, API keys)

2. **Authorization**
   - Budget service should verify the request is from Portfolio service
   - Validate user permissions

3. **Idempotency**
   - Deduct/Add operations should be idempotent
   - Use transaction IDs to prevent duplicate operations

4. **Error Handling**
   - Handle Budget service unavailability
   - Implement retries with exponential backoff
   - Consider circuit breaker pattern

---

## 📊 Database Schema

**Note:** Portfolio service does **NOT** have a UserBudgets table.  
Budget data is managed entirely by the Budget service.

Portfolio service only stores:
- **Positions** - User stock holdings
- **Transactions** - Buy/sell history

---

## 🎯 Example Workflow (After Integration)

```
1. User initiates BUY request
   └─> Portfolio API receives request

2. Portfolio service calls Budget service
   ├─> Check if user has sufficient funds
   └─> If yes, deduct funds from user's budget

3. If successful
   ├─> Portfolio service updates stock positions
   ├─> Records transaction in database
   └─> Returns success to user

4. If budget check fails
   └─> Return "Insufficient funds" error (no position created)
```

---

## 🚨 Important Notes

- ⚠️ **Currently NO budget validation** - all purchases succeed
- ⚠️ **Currently NO money tracking** - selling doesn't add funds
- ✅ **Ready for integration** - just replace the placeholder
- ✅ **Logs warnings** - check logs to see when placeholder is called

---

## 📞 Questions?

When integrating your Budget service, ensure:
1. Budget service endpoints match the interface methods
2. Error handling is robust
3. Transactions are atomic (use database transactions)
4. Consider using distributed transactions (Saga pattern) if needed

Good luck integrating! 🚀

