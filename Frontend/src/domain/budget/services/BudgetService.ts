import { categoriesService } from "./CategoriesService";
import { recurringTransactionsService } from "./RecurringTransactionsService";
import { spendingLimitsService } from "./SpendingLimitsService";
import { transactionsService } from "./TransactionsService";
import { walletsService } from "./WalletsService";

export const BudgetService = {
  categories: categoriesService,
  wallets: walletsService,
  transactions: transactionsService,
  recurring: recurringTransactionsService,
  limits: spendingLimitsService,
};