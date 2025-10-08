import { useState } from "react";
import { Button, Stack, Typography } from "@mui/material";
import TransactionFilters from "./components/TransactionFilters";
import TransactionsTable from "./components/TransactionsTable";
import { useTransactions } from "./hooks/useTransactions";
import TransactionsCreateDialog from "./components/TransactionsCreateDialog";

export default function TransactionsPage() {
  const { filter, setFilter, items, total, page, pageSize, loading } = useTransactions();
  const [openNew, setOpenNew] = useState(false);

  return (
    <div>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Typography variant="h4">All Transactions</Typography>
        <Button variant="contained" onClick={() => setOpenNew(true)}>New Transaction</Button>
      </Stack>

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

      <TransactionsCreateDialog
        open={openNew}
        onClose={() => setOpenNew(false)}
        onCreated={() => {

          setFilter(f => ({ ...f })); 
        }}
      />
    </div>
  );
}
