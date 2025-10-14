import { Guid } from "./budgetServiceTypes";

export type TransactionType = "Income" | "Expense";
export type TransactionSortBy = "createdAt" | "amount" | "date" | "type";
export type SortDir = "asc" | "desc";

export type CreateTransactionRequest = {
  userId: string | undefined;
  walletId: string;
  amount: number;         // send a positive number; backend derives sign from `type`
  currency: string;
  categoryName?: string;
  description?: string;
  date: string;      // ISO string
  type: TransactionType;  // <-- NEW
};

export type TransactionFilter = {
  walletId?: string;
  dateFrom?: string;      // yyyy-MM-dd
  dateTo?: string;        // yyyy-MM-dd
  categoryName?: string;
  type?: TransactionType; // NEW
  sortBy?: TransactionSortBy;
  sortDir?: SortDir;
  page?: number;
  pageSize?: number;
};

export interface Transaction {
  id: Guid;
  walletId: Guid;
  amount: number;
  currency: string;
  categoryName?: string | null;
  description?: string | null;
  date: string;
  type?: TransactionType;
}


export interface MonthlySummary {
  walletId: Guid; month: string;
  totalIncome: number; totalExpense: number;
  byCategory: Array<{ categoryName: string; amount: number }>;
}