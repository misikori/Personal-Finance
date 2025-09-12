# MarketGateway

Unified, config-driven **market data gateway** that exposes a normalized **gRPC API** (starting with `Quote`) and aggregates data from multiple vendors (e.g., AlphaVantage, Finnhub). Providers are plugged in via **YAML**, responses are **parsed and normalized**, and results are **cached** (SQLite for persistence; in-memory optional).

---

## Features (MVP)

- **gRPC API**: `Fetch` normalized market data (initially `Quote` by symbol).
- **Config-driven vendors**: YAML defines base URL, endpoints, query, and field mappings.
- **Provider orchestration**: Choose vendor(s) by type, handle HTTP, normalize responses.
- **Caching & storage**: SQLite via EF Core (file storage optional).
- **Logging & diagnostics**: Serilog + helpful startup checks.

---

## Repository Layout

- **MarketGateway.Contracts**: Protobuf contracts for internal usage
- **MarketGateway.Core**: gRPC host, DI, StartUp, Endpoints, Logic
- **MarketGateway.Providers**: API Configurations, Vendor loader
- **MarketGateway.Shared**: DTOs, utilities, mapping
- **MarketGateway.Data**: EF Core context, entities, migrations


## Quick Start (Development)
```bash
  dotnet build
```
```bash
  dotnet ef database update -p MarketGateway.Data -s MarketGateway.Core
```

```bash
    dotnet run --project MarketGateway.Core
```

## Add a New Data Type

1. Set new endpoint into ```` MarketGateway.Providers/Configurations/Vendors/````
2. Add new DataType (if needed) in ``` MarketGateway.Shared/Common.cs```
3. Add new DTO that follows endpoint definition in ```MarketGateway.Shared/DTOs```
4. Add it to ```MarketGateway.Shared/MarketDataResultFactory.cs```
5. Define Entity in ```MarketGateway.Data``` and set EF configuration file.
6. Create new Migration
7. Set new Contract in ```MarketGateway.Contracts```
