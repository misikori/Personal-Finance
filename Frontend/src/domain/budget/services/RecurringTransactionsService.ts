import { BaseService } from "../../../core/http/BaseService";
import { budgetApi } from "../../../core/http/apiClients";
import { CreateRecurringTransactionRequest, RecurringTransaction, Guid } from "../types/budgetServiceTypes";

class RecurringTransactionsService extends BaseService {
  constructor() { super(budgetApi, "/api/RecurringTransactions"); }
  create(body: CreateRecurringTransactionRequest) {
    return this.post<RecurringTransaction>("", body);
  }
  getByUser(userId: Guid) {
    return this.get<RecurringTransaction[]>(`/user/${userId}`);
  }
}
export const recurringTransactionsService = new RecurringTransactionsService();