import { BaseService } from "../../../core/http/BaseService";
import { budgetApi } from "../../../core/http/apiClients";
import { CreateRecurringTransactionRequest, RecurringTransaction, Guid } from "../types/budgetServiceTypes";
import { TransactionType } from "../types/transactionTypes";

class RecurringTransactionsService extends BaseService {

  
  constructor() { super(budgetApi, "/api/RecurringTransactions"); }


    private mapFromApi(item: any): RecurringTransaction {
    return {
      ...item,
      transactionType:
        item.transactionType === 1
          ? "Expense"
          : item.transactionType === 0
          ? "Income"
          : (item.transactionType as TransactionType),
    };
  }

  create(body: CreateRecurringTransactionRequest) {
    return this.post<RecurringTransaction>("", body);
  }
  async getByUser(userId: Guid) {
    const res = await this.get<RecurringTransaction[]>(`/user/${userId}`);
    return Array.isArray(res) ? res.map(this.mapFromApi) : [];
  }
}
export const recurringTransactionsService = new RecurringTransactionsService();