import { Typography } from "@mui/material";
import TransactionFilters from "./components/TransactionFilters";
import TransactionsTable from "./components/TransactionsTable";
import { useTransactions } from "./hooks/useTransactions";

export default function TransactionsPage() {
  const {
    filter, setFilter,
    items, total, page, pageSize, loading,
  } = useTransactions();

  return (
    <div>
      <Typography variant="h4" sx={{ mb: 2 }}>All Transactions</Typography>

      <TransactionFilters value={filter} onChange={setFilter} />

      <TransactionsTable
        rows={items}
        total={total}
        page={page}
        pageSize={pageSize}
        onPageChange={(p) => setFilter(f => ({ ...f, page: p }))}
        onPageSizeChange={(ps) => setFilter(f => ({ ...f, pageSize: ps, page: 1 }))}
        sortBy={filter.sortBy ?? "ts"}
        sortDir={filter.sortDir ?? "desc"}
        onSortChange={(by) => setFilter(f => ({
          ...f,
          sortBy: by,
          sortDir: f.sortBy === by ? (f.sortDir === "asc" ? "desc" : "asc") : "desc",
          page: 1
        }))}
        loading={loading}
        filter={filter}
      />
    </div>
  );
}
