import * as React from "react";
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Stack, MenuItem
} from "@mui/material";
import { useCurrencies } from "../../../../domain/currency/hooks/useCurrency";


export function WalletCreateDialog({
  open,
  onClose,
  onCreate,
  loading
}: {
  open: boolean;
  onClose: () => void;
  onCreate: (payload: { name: string; currency: string; startingBalance?: number }) => void;
  loading?: boolean;
}) {
  const [name, setName] = React.useState("");
  const [currency, setCurrency] = React.useState("USD");
  const [startingBalance, setStartingBalance] = React.useState<string>("");
  const allCurrencySymbols = useCurrencies("USD").currencies.map((c: { code: any; }) => c.code);

  const canSubmit = name.trim().length > 0 && currency;

  const handleSubmit = () => {
    onCreate({
      name: name.trim(),
      currency,
      startingBalance: startingBalance ? Number(startingBalance) : undefined,
    });
  };

  React.useEffect(() => {
    if (!open) {
      setName("");
      setCurrency("USD");
      setStartingBalance("");
    }
  }, [open]);

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>Create Wallet</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1 }}>
          <TextField
            label="Name"
            value={name}
            onChange={e => setName(e.target.value)}
            autoFocus
            fullWidth
            required
          />
          <TextField
            select
            label="Currency"
            value={currency}
            onChange={e => setCurrency(e.target.value)}
            fullWidth
            required
          >
            {allCurrencySymbols.map(ccy => (
              <MenuItem key={ccy} value={ccy}>{ccy}</MenuItem>
            ))}
          </TextField>
          <TextField
            label="Starting Balance (optional)"
            type="number"
            value={startingBalance}
            onChange={e => setStartingBalance(e.target.value)}
            fullWidth
            inputProps={{ step: "0.01" }}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button onClick={handleSubmit} variant="contained" disabled={!canSubmit || loading}>
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
}
