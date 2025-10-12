export type CurrencyCode = string; // e.g., "USD", "EUR"


export type TransactionSide = "Buy" | "Sell"; // backend returns string; we normalize

export interface TransactionDto {
id: string;
username: string;
symbol: string;
type: TransactionSide | string; // keep string to be safe
quantity: number;
pricePerShare: number;
currency: CurrencyCode;
totalValue: number;
transactionDate: string; // ISO
}

export interface PredictionDto {
symbol: string;
currentPrice: number;
predictedPrice: number;
predictedChangePercent: number;
confidence: number;
method: string;
generatedAt: string; // ISO
}

export interface TradeRequest {
username: string;
symbol: string;
quantity: number;
walletId: string;
}