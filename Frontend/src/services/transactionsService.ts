import { TRANSACTIONS_MOCK } from "../mocks/transactions.mock";
import type { PagedResult, Transaction, TransactionFilter } from "../types/transaction";

const delay = (ms=200) => new Promise(res => setTimeout(res, ms));

function matches(t: Transaction, f: TransactionFilter): boolean {
  if (f.dateFrom && new Date(t.ts) < new Date(f.dateFrom)) return false;
  if (f.dateTo && new Date(t.ts) > new Date(f.dateTo + "T23:59:59")) return false;
  if (f.types?.length && !f.types.includes(t.type)) return false;
  if (f.symbolContains?.trim()) {
    const needle = f.symbolContains.trim().toLowerCase();
    if (!(t.symbol ?? "").toLowerCase().includes(needle)) return false;
  }
  if (f.account && t.account !== f.account) return false;
  return true;
}

function sort(items: Transaction[], by: TransactionFilter["sortBy"], dir: "asc" | "desc") {
  const s = [...items].sort((a, b) => {
    if (by === "ts") return new Date(a.ts).getTime() - new Date(b.ts).getTime();
    if (by === "amount") return (a.amount ?? 0) - (b.amount ?? 0);
    return 0;
  });
  return dir === "asc" ? s : s.reverse();
}

export async function listTransactionsPaged(filter: TransactionFilter): Promise<PagedResult<Transaction>> {
  // Simulate network
  await delay();

  const {
    sortBy = "ts",
    sortDir = "desc",
    page = 1,
    pageSize = 10,
  } = filter;

  const filtered = TRANSACTIONS_MOCK.filter(t => matches(t, filter));
  const sorted = sort(filtered, sortBy, sortDir);
  const start = (page - 1) * pageSize;
  const items = sorted.slice(start, start + pageSize);

  return { items, total: filtered.length, page, pageSize };
}
