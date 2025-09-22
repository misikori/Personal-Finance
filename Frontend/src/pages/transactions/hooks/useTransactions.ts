import { useCallback, useEffect, useState } from "react";
import type { Transaction, TransactionFilter, PagedResult } from "../../../types/transaction";
import { listTransactionsPaged } from "../../../services/transactionsService";

const DEFAULTS: Required<Pick<TransactionFilter, "sortBy" | "sortDir" | "page" | "pageSize">> = {
  sortBy: "ts",
  sortDir: "desc",
  page: 1,
  pageSize: 10,
};

export function useTransactions(initial?: TransactionFilter) {
  const [filter, setFilter] = useState<TransactionFilter>({ ...DEFAULTS, ...initial });
  const [data, setData] = useState<PagedResult<Transaction>>({ items: [], total: 0, page: 1, pageSize: 10 });
  const [loading, setLoading] = useState(false);

  const load = useCallback(async (f: TransactionFilter) => {
    setLoading(true);
    const res = await listTransactionsPaged(f);
    setData(res);
    setLoading(false);
  }, []);

  useEffect(() => {
    const id = setTimeout(() => load(filter), 120);
    return () => clearTimeout(id);
  }, [filter, load]);

  return {
    filter,
    setFilter,
    ...data,
    loading,
  };
}
