import { BaseService } from "../../../core/http/BaseService";
import { budgetApi } from "../../../core/http/apiClients";
import { Guid } from "../types/budgetServiceTypes";
import { CreateTransactionRequest, MonthlySummary, Transaction, TransactionType } from "../types/transactionTypes";

class TransactionsService extends BaseService {
  constructor() { super(budgetApi, ""); }


  list(walletId: Guid, params?: {
    startDate?: string;
    endDate?: string;
    categoryName?: string;
    type?: TransactionType
  }) {
    return this.get<Transaction[]>(`/api/wallets/${walletId}/transactions`, params);
  }
  create(body: CreateTransactionRequest) {
    return this.post<Transaction>("/api/Transaction", body);
  }
  monthlySummary(walletId: Guid) {
    return this.get<MonthlySummary>(`/api/wallets/${walletId}/summary/monthly`);
  }
}
export const transactionsService = new TransactionsService();