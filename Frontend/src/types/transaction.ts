export type TransactionType =
  | "BUY" | "SELL" | "DIV" | "DEPOSIT" | "WITHDRAWAL" | "FEE";

export type Transaction = {
  id: string;
  ts: string;           // ISO
  type: TransactionType;
  symbol?: string;
  qty?: number;
  price?: number;
  fees?: number;
  amount: number;       // cash impact (+/-)
  account: string;      // e.g. "Brokerage", "IRA"
  currency?: string;    // USD default
};

export type TransactionSortBy = "ts" | "amount";

export type TransactionFilter = {
  dateFrom?: string;    // "YYYY-MM-DD" 
  dateTo?: string;      // "YYYY-MM-DD"
  types?: TransactionType[];
  symbolContains?: string;
  account?: string;
  sortBy?: TransactionSortBy;
  sortDir?: "asc" | "desc";
  page?: number;        
  pageSize?: number;  
};

export type PagedResult<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};
