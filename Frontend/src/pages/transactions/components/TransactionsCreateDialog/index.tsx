import { useEffect, useMemo, useState } from "react";
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, MenuItem, FormControlLabel, Switch, Stack
} from "@mui/material";
import { decodeJwt, getEmailFromPayload } from "../../../../auth/jwt";
import { authStore, getCurrentUser } from "../../../../auth/store/authStore";
import { categoriesService } from "../../../../domain/budget/services/CategoriesService";
import { recurringTransactionsService } from "../../../../domain/budget/services/RecurringTransactionsService";
import { transactionsService } from "../../../../domain/budget/services/TransactionsService";
import { walletsService } from "../../../../domain/budget/services/WalletsService";
import { Wallet, Category } from "../../../../domain/budget/types/budgetServiceTypes";
import type { TransactionType } from "../../../../domain/budget/types/transactionTypes";

type Props = { open: boolean; onClose: () => void; onCreated?: () => void; };

export default function TransactionsCreateDialog({ open, onClose, onCreated }: Props) {
  const [wallets, setWallets] = useState<Wallet[]>([]);
  const [cats, setCats] = useState<Category[]>([]);
  const [loading, setLoading] = useState(false);
  const [isRecurring, setIsRecurring] = useState(false);

  const [form, setForm] = useState({
    walletId: "",
    amount: "",
    currency: "",
    categoryName: "",
    description: "",
    dateLocal: new Date().toISOString().slice(0, 16), // yyyy-MM-ddTHH:mm
    type: "Expense" as TransactionType,
    // recurring
    cadence: "Monthly",
    nextRun: new Date().toISOString().slice(0, 16),
  });

  useEffect(() => {
    if (!open) return;
    let mounted = true;
    (async () => {
      try {
        const userId = getCurrentUser()?.id || "";
        const [ws, cs] = await Promise.all([
          walletsService.getByUser(userId),
          categoriesService.listByUser(userId),
        ]);
        if (!mounted) return;
        setWallets(ws);
        setCats(cs);
        if (ws.length) {
          setForm(f => ({ ...f, walletId: ws[0].id, currency: ws[0].currency }));
        }
      } catch {}
    })();
    return () => { mounted = false; };
  }, [open]);

  const walletOptions = useMemo(
    () => wallets.map(w => <MenuItem key={w.id} value={w.id}>{w.name} ({w.currency})</MenuItem>),
    [wallets]
  );

  const submit = async () => {
    setLoading(true);
    try {
      const amount = Math.abs(Number(form.amount));
      const currentUser = getCurrentUser();
      // fallback: try JWT sub if your store lacks id (optional)
      const fallbackUserId = (() => {
        try { return (decodeJwt(authStore.accessToken)?.sub as string) || ""; } catch { return ""; }
      })();
      const userId = currentUser?.id || fallbackUserId;

      // Convert `datetime-local` (local wall time) -> ISO Z
      // This preserves the exact instant the user picked.
      const dateIso = new Date(form.dateLocal).toISOString();

      if (!isRecurring) {
        await transactionsService.create({
          walletId: form.walletId,
          amount,
          type: form.type,
          description: form.description || undefined,
          date: dateIso, // <-- API expects `date`
          currency: form.currency || (wallets.find(w => w.id === form.walletId)?.currency ?? "USD"),
          categoryName: form.categoryName || undefined,
        });
      } else {
        const p = decodeJwt(authStore.accessToken);
        const emailUserId = getEmailFromPayload(p) as string; // if your recurring API wants email; adjust if it needs GUID
        await recurringTransactionsService.create({
          userId: emailUserId,
          walletId: form.walletId,
          amount,
          currency: form.currency || (wallets.find(w => w.id === form.walletId)?.currency ?? "USD"),
          description: form.description || undefined,
          categoryName: form.categoryName || undefined,
          cadence: form.cadence as any,
          nextRun: new Date(form.nextRun).toISOString(),
          // add `type: form.type` here if your backend supports it for recurring
        } as any);
      }
      onClose();
      onCreated?.();
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>New {isRecurring ? "Recurring " : ""}Transaction</DialogTitle>
      <DialogContent dividers sx={{ pt: 2 }}>
        <Stack spacing={2}>
          <FormControlLabel
            control={<Switch checked={isRecurring} onChange={e => setIsRecurring(e.target.checked)} />}
            label="Recurring"
          />
          <TextField
            select label="Wallet" value={form.walletId}
            onChange={(e) => {
              const walletId = String(e.target.value);
              const currency = wallets.find(w => w.id === walletId)?.currency ?? form.currency;
              setForm(f => ({ ...f, walletId, currency }));
            }}
          >{walletOptions}</TextField>

          <TextField
            select
            label="Type"
            value={form.type}
            onChange={(e) => setForm(f => ({ ...f, type: e.target.value as TransactionType }))}
          >
            {["Income", "Expense"].map(t => <MenuItem key={t} value={t}>{t}</MenuItem>)}
          </TextField>

          <TextField
            label="Amount"
            type="number"
            inputProps={{ step: "0.01" }}
            value={form.amount}
            onChange={(e) => setForm(f => ({ ...f, amount: e.target.value }))}
          />

          <TextField
            label="Currency"
            value={form.currency}
            onChange={(e) => setForm(f => ({ ...f, currency: e.target.value }))}
          />

          <TextField
            label="Category"
            value={form.categoryName}
            onChange={(e) => setForm(f => ({ ...f, categoryName: String(e.target.value) }))}
          />
          <TextField
            label="Description"
            value={form.description}
            onChange={(e) => setForm(f => ({ ...f, description: String(e.target.value) }))}
          />

          <TextField
            label="Date & Time"
            type="datetime-local"
            InputLabelProps={{ shrink: true }}
            value={form.dateLocal}
            onChange={(e) => setForm(f => ({ ...f, dateLocal: e.target.value }))}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button
          onClick={submit}
          variant="contained"
          disabled={loading || !form.walletId || !form.amount}
        >
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
}
