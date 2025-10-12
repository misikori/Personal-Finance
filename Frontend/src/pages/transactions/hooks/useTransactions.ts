import { useCallback, useEffect, useState } from "react";
import { BudgetService } from "../../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../../auth/store/authStore";
import type {
  Transaction,
  TransactionFilter,
  TransactionSortBy,
} from "../../../domain/budget/types/transactionTypes";

type PagedResult<T> = { items: T[]; total: number; page: number; pageSize: number };

const DEFAULTS: Required<Pick<TransactionFilter, "sortBy" | "sortDir" | "page" | "pageSize">> = {
  sortBy: "date",
  sortDir: "desc",
  page: 1,
  pageSize: 10,
};

export function useTransactions(initial?: TransactionFilter) {
  const [filter, setFilter] = useState<TransactionFilter>({ ...DEFAULTS, ...initial });
  const [data, setData] = useState<PagedResult<Transaction>>({
    items: [],
    total: 0,
    page: DEFAULTS.page,
    pageSize: DEFAULTS.pageSize,
  });
  const [loading, setLoading] = useState(false);
  const [wallets, setWallets] = useState<Array<{ id: string; name: string; currency: string }>>([]);

  const load = useCallback(async (f: TransactionFilter) => {
    setLoading(true);
    try {
      const userId = getCurrentUser()?.id;
      if (!userId) {
        setData({
          items: [],
          total: 0,
          page: f.page ?? DEFAULTS.page,
          pageSize: f.pageSize ?? DEFAULTS.pageSize,
        });
        setWallets([]);
        return;
      }

      // Always fetch wallets to display wallet names
      const userWallets = await BudgetService.wallets.getByUser(userId);
      setWallets(userWallets);

      let raw: Transaction[] = [];

      // If no wallet selected, fetch from ALL wallets
      if (!f.walletId) {
        // Fetch transactions from all wallets and merge
        const allTransactions = await Promise.all(
          userWallets.map(wallet => 
            BudgetService.transactions.list(wallet.id, {
              startDate: f.dateFrom,
              endDate: f.dateTo,
              categoryName: f.categoryName,
            })
          )
        );
        
        raw = allTransactions.flat();
      } else {
        // Fetch from specific wallet only
        raw = await BudgetService.transactions.list(f.walletId, {
          startDate: f.dateFrom,
          endDate: f.dateTo,
          categoryName: f.categoryName,
        });
      }

      const afterType = f.type ? raw.filter(t => t.type === f.type) : raw;

      const dir = f.sortDir === "asc" ? 1 : -1;
      const sorted = [...afterType].sort((a, b) => {
        const by: TransactionSortBy = (f.sortBy ?? DEFAULTS.sortBy) as TransactionSortBy;

        if (by === "date") {
          return (new Date(a.date).getTime() - new Date(b.date).getTime()) * dir;
        }
        if (by === "amount") {
          return (a.amount - b.amount) * dir;
        }
        if (by === "type") {
          return (a.type ?? "").localeCompare(b.type ?? "") * dir;
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

  return { filter, setFilter, ...data, loading, wallets };
}

