import * as React from "react";
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Stack, MenuItem
} from "@mui/material";
import { useWallets } from "../../transactions/hooks/useWallets";
import { BudgetService } from "../../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../../auth/store/authStore";
import { TransactionType } from "../../../domain/budget/types/transactionTypes";


type Frequency = "Weekly" | "Monthly" | "Yearly";
const transactionTypes: TransactionType[] = ["Expense", "Income"];
const frequencies: Frequency[] = ["Weekly", "Monthly", "Yearly"];

export default function RecurringTransactionCreateDialog({ open, onClose, onCreated }: {
  open: boolean;
  onClose: () => void;
  onCreated: () => void;
}) {
  const { wallets } = useWallets();
  const [amount, setAmount] = React.useState("");
  const [currency, setCurrency] = React.useState(wallets[0]?.currency || "");
  const [walletId, setWalletId] = React.useState(wallets[0]?.id || "");
  const [description, setDescription] = React.useState("");
  const [transactionType, setTransactionType] = React.useState<TransactionType>("Expense");
  const [frequency, setFrequency] = React.useState<Frequency>("Monthly");
  const [startDate, setStartDate] = React.useState("");
  const [endDate, setEndDate] = React.useState("");
  const [loading, setLoading] = React.useState(false);

  const handleCreate = async () => {
    setLoading(true);
    try {
      const userId = getCurrentUser()?.id;
      if (!userId) throw new Error("User not found");
      await BudgetService.recurring.create({
          userId,
          walletId,
          amount: Number(amount),
          TransactionType: transactionType,
          description,
          currency,
          frequency,
          startDate,
          endDate,
          nextRun: ""
      });
      onCreated();
      onClose();
    } finally {
      setLoading(false);
    }
  };

  React.useEffect(() => {
    if (open && wallets.length) {
      setCurrency(wallets[0].currency);
      setWalletId(wallets[0].id);
    }
  }, [open, wallets]);

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Add Recurring Transaction</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            select
            label="Wallet"
            value={walletId}
            onChange={e => setWalletId(e.target.value)}
            fullWidth
            required
          >
            {wallets.map(w => (
              <MenuItem key={w.id} value={w.id}>{w.name} ({w.currency})</MenuItem>
            ))}
          </TextField>
          <TextField
            label="Amount"
            type="number"
            value={amount}
            onChange={e => setAmount(e.target.value)}
            fullWidth
            required
          />
          <TextField
            label="Currency"
            value={currency}
            onChange={e => setCurrency(e.target.value)}
            fullWidth
            required
          />
          {/* Category fields removed as not in DTO */}
          <TextField
            label="Description"
            value={description}
            onChange={e => setDescription(e.target.value)}
            fullWidth
          />
          <TextField
            select
            label="Type"
            value={transactionType}
            onChange={e => setTransactionType(e.target.value as TransactionType)}
            fullWidth
          >
            {transactionTypes.map(type => (
              <MenuItem key={type} value={type}>{type}</MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Frequency"
            value={frequency}
            onChange={e => setFrequency(e.target.value as Frequency)}
            fullWidth
          >
            {frequencies.map(freq => (
              <MenuItem key={freq} value={freq}>{freq}</MenuItem>
            ))}
          </TextField>
          {/* nextRun removed as not in DTO */}
          <TextField
            label="Start Date"
            type="date"
            value={startDate}
            onChange={e => setStartDate(e.target.value)}
            fullWidth
          />
          <TextField
            label="End Date"
            type="date"
            value={endDate}
            onChange={e => setEndDate(e.target.value)}
            fullWidth
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button onClick={handleCreate} variant="contained" disabled={loading}>Add</Button>
      </DialogActions>
    </Dialog>
  );
}
