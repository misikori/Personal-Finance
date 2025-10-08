import { TransactionType } from "./transactionTypes";

export type Guid = string;

export interface Category {
  id: Guid; name: string; userId: Guid;
  icon?: string | null; parentId?: Guid | null;
}
export interface CreateCategoryRequest {
  name: string; userId: Guid; icon?: string | null; parentId?: Guid | null;
}

export interface Wallet {
  id: Guid; userId: Guid; name: string; currency: string; createdAt?: string;   balance?: number;
}
export interface CreateWalletRequest { userId: Guid; name: string; currency: string; }

export type RecurrenceFrequency =  "Weekly" | "Monthly" | "Yearly"




export interface RecurringTransaction {
  id: Guid;
  userId: Guid; 
  walletId: Guid; 
  amount: number; 
  TransactionType: TransactionType;
  currency: string;
  RecurrenceFrequency: RecurrenceFrequency;
  nextRun: string; 
  description?: string | null;
  categoryId?: Guid | null;
  categoryName?: string | null;
  startDate: string;
  endDate:string;
}
export type CreateRecurringTransactionRequest =
  Omit<RecurringTransaction, "id">;

export interface SpendingLimit {
  id: Guid; walletId: Guid;
  categoryId?: Guid | null; categoryName?: string | null;
  amount: number; currency: string; period: "Monthly" | "Weekly"; startsOn: string;
}
export type CreateSpendingLimitRequest = Omit<SpendingLimit, "id">;
