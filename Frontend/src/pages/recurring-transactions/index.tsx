import { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, Typography, Stack, Button } from "@mui/material";
import { BudgetService } from "../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../auth/store/authStore";
import { RecurringTransaction } from "../../domain/budget/types/budgetServiceTypes";
import RecurringTransactionCreateDialog from "./components/RecurringTransactionCreateDialog";


export default function RecurringTransactionsPage() {
  const [rows, setRows] = useState<RecurringTransaction[]>([]);
  const [loading, setLoading] = useState(false);
  const [openNew, setOpenNew] = useState(false);

  const load = async () => {
    setLoading(true);
    const userId = getCurrentUser()?.id;
    if (!userId) return;
    const res = await BudgetService.recurring.getByUser(userId); 
    console.log(res);
    setRows(res ?? []);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  return (
    <Stack spacing={2}>
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2 }}>
        <Typography variant="h6">Recurring Transactions</Typography>
        <Button variant="contained" onClick={() => setOpenNew(true)}>Add Recurring Transaction</Button>
      </Stack>
      {loading ? (
        <Typography>Loading…</Typography>
      ) : rows.length === 0 ? (
        <Typography color="text.secondary">No recurring transactions.</Typography>
      ) : (
        rows.map(r => (
          <Card key={r.id} variant="outlined">
            <CardHeader title={`${r.id} • ${r.categoryName ?? "—"}`} subheader={r.description ?? "—"} />
            <CardContent>
              <Typography>
                Amount: {r.TransactionType === "Expense" ? "- " : ""}{r.amount} {r.currency}
              </Typography>
              {r.nextRun && <Typography>Schedule: {r.nextRun}</Typography>}
              {r.nextRun && <Typography>Next run: {new Date(r.nextRun).toLocaleString()}</Typography>}
            </CardContent>
          </Card>
        ))
      )}
      <RecurringTransactionCreateDialog
        open={openNew}
        onClose={() => setOpenNew(false)}
        onCreated={load}
      />
    </Stack>
  );
}
