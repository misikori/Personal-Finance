import { CurrencyCode } from "./transaction";

export interface PositionDto {
symbol: string;
quantity: number;
averagePurchasePrice: number;
currentPrice: number;
currency: CurrencyCode;
totalInvested: number;
currentValue: number;
gainLoss: number;
gainLossPercentage: number;
firstPurchaseDate: string; // ISO
}

export interface PortfolioSummaryDto {
username: string;
baseCurrency: CurrencyCode;
totalInvested: number;
currentValue: number;
totalGainLoss: number;
gainLossPercentage: number;
positions: PositionDto[];
}

export interface PortfolioDistributionItemDto {
symbol: string;
quantity: number;
value: number;
percentage: number; // 0..100 (assumption based on description)
originalCurrency: CurrencyCode;
currentPrice: number;
currency: CurrencyCode; // converted currency (baseCurrency)
color: string; // hex or name, provided by API
}

export interface PortfolioDistributionDto {
username: string;
baseCurrency: CurrencyCode;
totalValue: number;
holdings: PortfolioDistributionItemDto[];
}