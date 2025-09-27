# 💱 Currency Service

This service provides currency rates via REST API and gRPC.  
It uses Redis as a storage backend for caching exchange rates.

---

## 📦 Setup

1. Start Redis with Docker:
   ```bash
   docker run --name redis -p 6379:6379 -d redis
   ```

2. Update `appsettings.json` with the correct rates URL:
   ```json
   {
     "CurrencyRatesUrl": "https://kurs.resenje.org/api/v1/rates/today"
   }
   ```

3. Run the service

> **Note**  
> Current setup is intended only for local development and testing.  
> In the future, Redis and services will be orchestrated via **Docker Compose**, and manual setup should no longer be required.
---

## 🌐 REST API Endpoints

Base URL:
```
http://localhost:5044/api/currency
```

### 1. Get all rates
**Request**
```http
GET /api/currency
```

**Query parameters**
- `baseCurrencyCode` *(optional)* – e.g. `EUR`. Defaults to `RSD`.

**Responses**
- `200 OK` → JSON list of currency rates  
- `404 Not Found` → if no rates are available  
- `400 Bad Request` → invalid baseCurrencyCode (if provided as query parameter)

---

### 2. Convert amount
**Request**
```http
GET /api/currency/convert?from=USD&to=EUR&amount=100
```

**Query parameters**
- `from` *(required)* – source currency code  
- `to` *(required)* – target currency code  
- `amount` *(required)* – decimal amount to convert  

**Responses**
- `200 OK` → JSON with conversion result:
  ```json
  {
    "from": "USD",
    "to": "EUR",
    "amount": 100,
    "rate": 0.92,
    "converted": 92
  }
  ```
- `404 Not Found` → if currency not found  
- `503 Service Unavailable` → if rates unavailable  

---

### 3. Update rates
**Request**
```http
PUT /api/currency
Content-Type: application/json
```

**Body**
```json
{
  "Key": "rates:global",
  "Rates": [
    {
      "Code": "USD",
      "ExchangeMiddle": 106.5,
      "Parity": 1,
      "Date": "2025-08-31"
    }
  ]
}
```

**Response**
- `200 OK` → Updated list of rates  

---

### 4. Delete rates
**Request**
```http
DELETE /api/currency
```

**Response**
- `200 OK` → when rates deleted successfully  

---

## ⚡ gRPC Endpoints

Service definition: `CurrencyRatesProtoService`

### 1. GetConversion
**Request (GetConversionRequest)**

- `from` *(required)* – source currency code  
- `to` *(required)* – target currency code  
- `amount` *(required)* – decimal amount to convert  

```json
{
  "from": "USD",
  "to": "EUR",
  "amount": 100
}
```

**Response (GetConversionResponse)**
```json
{
  "from": "USD",
  "to": "EUR",
  "amount": 100,
  "rate": 0.92,
  "converted": 92
}
```

**Errors**
- `NOT_FOUND` → if conversion not possible  

---

### 2. GetSpecificCurrencyRates
**Request (GetSpecificCurrencyRatesRequest)**

- `baseCurrencyCode` *(optional)* – e.g. `EUR`. Defaults to `RSD`.

```json
{
  "baseCurrency": "EUR"
}
```

**Response (GetSpecificCurrencyRatesResponse)**
```json
{
  "rates": [
    {
      "code": "USD",
      "date": "2025-08-31T00:00:00Z",
      "parity": 1,
      "exchangeMiddle": 1.09
    }
  ]
}
```

**Errors**
- `NOT_FOUND` → if base currency is invalid  

---
