import { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, Typography, Stack } from "@mui/material";
import { BudgetService } from "../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../auth/store/authStore";
import { RecurringTransaction } from "../../domain/budget/types/budgetServiceTypes";


export default function RecurringTransactionsPage() {
  const [rows, setRows] = useState<RecurringTransaction[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    (async () => {
      setLoading(true);
      const userId = getCurrentUser()?.id;
      const res = await BudgetService.recurring.getByUser(userId!); 
      setRows(res ?? []);
      setLoading(false);
    })();
  }, []);

  return (
    <Stack spacing={2}>
      <Typography variant="h6">Recurring Transactions</Typography>
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
    </Stack>
  );
}
