export interface Currency {
  code: string;
  date: string;
  parity: number;
  exchangeMiddle: number;
}

export interface CurrencyConvertRequest {
  from: string;
  to: string;
  amount: number;
  rate?: number;
}

export interface CurrencyConvertResponse {
  from: string;
  to: string;
  amount: number;
  rate: number;
  converted: number;
}
