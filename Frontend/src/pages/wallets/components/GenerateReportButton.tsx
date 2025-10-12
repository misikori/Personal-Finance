import { Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, MenuItem } from "@mui/material";
import { useState } from "react";
import { BudgetService } from "../../../domain/budget/services/BudgetService";
import { useWallets } from "../../transactions/hooks/useWallets";


export function GenerateReportButton({ userId, username, emailAddress }: {
  userId: string;
  username: string;
  emailAddress: string;
}) {
  const { wallets } = useWallets();
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [walletId, setWalletId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const handleOpen = () => setOpen(true);
  const handleClose = () => setOpen(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const payload = { userId, walletId, username, emailAddress, startDate, endDate };
      const result = await BudgetService.reports.transactionsReport(payload);
      alert("Report generated! Check console for result.");
      console.log(result);
      setOpen(false);
    } catch (e) {
      alert("Failed to generate report");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Button variant="contained" size="small" onClick={handleOpen}>
        Generate Report
      </Button>
      <Dialog open={open} onClose={handleClose}>
        <DialogTitle>Generate Transactions Report</DialogTitle>
        <form onSubmit={handleSubmit}>
          <DialogContent>
            <TextField
              select
              label="Wallet"
              value={walletId}
              onChange={e => setWalletId(e.target.value)}
              fullWidth
              margin="normal"
              required
            >
              {wallets.map(w => (
                <MenuItem key={w.id} value={w.id}>{w.name} ({w.currency})</MenuItem>
              ))}
            </TextField>
            <TextField
              label="Start Date"
              type="date"
              value={startDate}
              onChange={e => setStartDate(e.target.value)}
              fullWidth
              margin="normal"
              InputLabelProps={{ shrink: true }}
              required
            />
            <TextField
              label="End Date"
              type="date"
              value={endDate}
              onChange={e => setEndDate(e.target.value)}
              fullWidth
              margin="normal"
              InputLabelProps={{ shrink: true }}
              required
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose} disabled={loading}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={loading || !walletId || !startDate || !endDate}>
              Generate
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  );
}
