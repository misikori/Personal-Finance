import { TransactionType } from "./transactionTypes";

export type CreateRecurringTransactionRequest = {
  userId: string;
  walletId: string;
  amount: number;         // positive; backend uses `type`
  currency: string;
  description?: string;
  categoryName?: string;
  cadence: "Daily" | "Weekly" | "Monthly" | "Yearly";
  nextRun: string;        // ISO string
  type: TransactionType;  // <-- NEW
};