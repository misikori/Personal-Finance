import { useCallback, useEffect, useState } from "react";
import { BudgetService } from "../../../domain/budget/services/BudgetService";
import type {
  Transaction, TransactionFilter, TransactionSortBy
} from "../../../domain/budget/types/transactionTypes";

type PagedResult<T> = { items: T[]; total: number; page: number; pageSize: number };

const DEFAULTS: Required<Pick<TransactionFilter, "sortBy" | "sortDir" | "page" | "pageSize">> = {
  sortBy: "createdAt",
  sortDir: "desc",
  page: 1,
  pageSize: 10,
};

export function useTransactions(initial?: TransactionFilter) {
  const [filter, setFilter] = useState<TransactionFilter>({ ...DEFAULTS, ...initial });
  const [data, setData] = useState<PagedResult<Transaction>>({
    items: [], total: 0, page: DEFAULTS.page, pageSize: DEFAULTS.pageSize
  });
  const [loading, setLoading] = useState(false);

  const load = useCallback(async (f: TransactionFilter) => {
    if (!f.walletId) {
      setData({ items: [], total: 0, page: f.page ?? 1, pageSize: f.pageSize ?? 10 });
      return;
    }
    setLoading(true);
    try {
      const raw = await BudgetService.transactions.list(f.walletId, {
        startDate: f.dateFrom,
        endDate: f.dateTo,
        categoryName: f.categoryName,
        type: f.type,
      });

      const dir = f.sortDir === "asc" ? 1 : -1;
      const sorted = [...raw].sort((a, b) => {
        if (f.sortBy === "createdAt") {
          return (new Date(a.date).getTime() - new Date(b.date).getTime()) * dir;
        }
        if (f.sortBy === "amount") {
          return (a.amount - b.amount) * dir;
        }
        return 0;
      });

      const page = f.page ?? DEFAULTS.page;
      const pageSize = f.pageSize ?? DEFAULTS.pageSize;
      const start = (page - 1) * pageSize;
      const items = sorted.slice(start, start + pageSize);

      setData({ items, total: sorted.length, page, pageSize });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    let cancelled = false;
    const id = setTimeout(async () => {
      if (!cancelled) await load(filter);
    }, 150);
    return () => { cancelled = true; clearTimeout(id); };
  }, [filter, load]);

  return { filter, setFilter, ...data, loading };
}
