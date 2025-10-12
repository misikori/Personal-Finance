import { categoriesService } from "./CategoriesService";
import { recurringTransactionsService } from "./RecurringTransactionsService";
import { spendingLimitsService } from "./SpendingLimitsService";
import { transactionsService } from "./TransactionsService";
import { walletsService } from "./WalletsService";
import { budgetApi } from "../../../core/http/apiClients";

export const BudgetService = {
  categories: categoriesService,
  wallets: walletsService,
  transactions: transactionsService,
  recurring: recurringTransactionsService,
  limits: spendingLimitsService,
  reports: {
    async transactionsReport(payload: any) {
      // You can define a type for payload if needed
      const response = await budgetApi.post("/api/Reports/transactions", payload);
      return response.data;
    },
  },
};