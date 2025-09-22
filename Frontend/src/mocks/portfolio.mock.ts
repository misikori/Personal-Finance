import type { Position } from "../types/portfolio";

export const POSITIONS_MOCK: Position[] = [
  { symbol: "AAPL", name: "Apple Inc.",    qty: 40,  avgCost: 155.00, last: 195.10, marketValue: 0, unrealizedAbs: 0, unrealizedPct: 0, sector: "Technology", class: "Equity" },
  { symbol: "MSFT", name: "Microsoft",     qty: 25,  avgCost: 285.00, last: 420.50, marketValue: 0, unrealizedAbs: 0, unrealizedPct: 0, sector: "Technology", class: "Equity" },
  { symbol: "NVDA", name: "NVIDIA",        qty: 12,  avgCost: 480.00, last: 830.20, marketValue: 0, unrealizedAbs: 0, unrealizedPct: 0, sector: "Technology", class: "Equity" },
  { symbol: "XOM",  name: "Exxon Mobil",   qty: 30,  avgCost: 95.00,  last: 112.30, marketValue: 0, unrealizedAbs: 0, unrealizedPct: 0, sector: "Energy",     class: "Equity" },
  { symbol: "JNJ",  name: "Johnson & J.",  qty: 18,  avgCost: 154.00, last: 161.40, marketValue: 0, unrealizedAbs: 0, unrealizedPct: 0, sector: "Healthcare", class: "Equity" },
  { symbol: "VOO",  name: "Vanguard S&P500 ETF", qty: 10, avgCost: 400.00, last: 515.00, marketValue: 0, unrealizedAbs: 0, unrealizedPct: 0, sector: "Index", class: "ETF" },
];