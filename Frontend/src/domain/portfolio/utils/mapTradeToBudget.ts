import type { TransactionDto } from "../types/transaction";
import type { CreateTransactionRequest, TransactionType } from "../../budget/types/transactionTypes";
import type { Guid } from "../../budget/types/budgetServiceTypes";

const CATEGORY_DEFAULT = "Investments"; // change if you have a specific category

export function mapTradeToBudget(
  trade: TransactionDto,
  walletId: Guid
): CreateTransactionRequest {
  const isSell = String(trade.type).toLowerCase().includes("sell");
  const txType: TransactionType = isSell ? "Income" : "Expense";

  // Budget amount: positive for Income, positive number for Expense too (backend derives sign from type)
  // Your earlier note: "send a positive number; backend derives sign from `type`"
  const amount = Math.abs(trade.totalValue);

  return {
    walletId: String(walletId),
    amount,
    currency: trade.currency,
    categoryName: CATEGORY_DEFAULT,
    description: `${isSell ? "Sell" : "Buy"} ${trade.quantity} ${trade.symbol} @ ${trade.pricePerShare}`,
    date: trade.transactionDate,
    type: txType,
  };
}
