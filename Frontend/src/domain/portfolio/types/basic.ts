import { CurrencyCode } from "./transaction";

export interface PriceQuoteDto {
symbol: string;
price: number;
open: number;
high: number;
low: number;
previousClose: number;
volume: number;
currency: CurrencyCode;
asOf: string; // ISO
}

export interface CandleDto {
date: string; // ISO
open: number;
high: number;
low: number;
close: number;
volume: number;
change: number;
changePercent: number;
}

export interface CandlestickResponseDto {
symbol: string;
startDate: string; // ISO
endDate: string; // ISO
count: number;
data: CandleDto[];
}