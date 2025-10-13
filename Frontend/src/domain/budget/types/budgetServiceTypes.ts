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
  id: Guid; userId: Guid; name: string; currency: string; createdAt?: string;   currentBalance: number;
}
export interface CreateWalletRequest { userId: Guid; name: string; currency: string; initialBalance?: number; }

export type RecurrenceFrequency =  "Weekly" | "Monthly" | "Yearly"




export interface RecurringTransaction {
  id: Guid;
  userId: Guid; 
  walletId: Guid; 
  amount: number; 
  transactionType: TransactionType;
  currency: string;
  frequency: RecurrenceFrequency;
  nextDueDate: string; 
  description?: string | null;
  category?: string | null;
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
