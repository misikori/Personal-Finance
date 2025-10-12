import * as React from "react";
import {
  Card, CardContent, CardHeader, IconButton, List, ListItem, ListItemText,
  Stack, Typography, Divider, Button, Tooltip
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import AccountBalanceWalletIcon from "@mui/icons-material/AccountBalanceWallet";
import { getCurrentUser } from "../../../../auth/store/authStore";
import { BudgetService } from "../../../../domain/budget/services/BudgetService";
import { Wallet, } from "../../../../domain/budget/types/budgetServiceTypes";
import { WalletCreateDialog } from "../WalletCreateDialog";
import { formatNumber } from "../../../../shared/utils/format";


export function WalletsPanel() {
  const user  = getCurrentUser();
  const [wallets, setWallets] = React.useState<Wallet[]>([]);
  const [open, setOpen] = React.useState(false);
  const [loading, setLoading] = React.useState(false);

  const load = React.useCallback(async () => {
    if (!user?.id) return;
    setLoading(true);
    try {
      const res = await BudgetService.wallets.getByUser(user.id);
      setWallets(res ?? []);
    } finally {
      setLoading(false);
    }
  }, [user?.id]);

  React.useEffect(() => { load(); }, [load]);

const onCreate = async (payload: { name: string; currency: string; }) => {
  if (!user?.id) return;
  await BudgetService.wallets.create({
    userId: user.id,
    name: payload.name,
    currency: payload.currency,
  });
  setOpen(false);
  await load();
};

  const totalsByCurrency = React.useMemo(() => {
    const acc: Record<string, number> = {};
    for (const w of wallets) {
      acc[w.currency] = (acc[w.currency] ?? 0) + (w.currentBalance ?? 0);
    }
    return acc;
  }, [wallets]);

  return (
    <Card variant="outlined" sx={{ borderRadius: 3, height: "100%" }}>
      <CardHeader
        title={<Stack direction="row" alignItems="center" gap={1}>
          <AccountBalanceWalletIcon fontSize="small" />
          <Typography variant="subtitle1" fontWeight={700}>Wallets</Typography>
        </Stack>}
        action={
          <Tooltip title="Create wallet">
            <IconButton onClick={() => setOpen(true)} aria-label="create-wallet">
              <AddIcon />
            </IconButton>
          </Tooltip>
        }
      />
      <CardContent>
        {wallets.length === 0 ? (
          <Stack spacing={1} alignItems="flex-start">
            <Typography variant="body2" color="text.secondary">
              No wallets yet.
            </Typography>
            <Button startIcon={<AddIcon />} variant="contained" size="small" onClick={() => setOpen(true)}>
              New Wallet
            </Button>
          </Stack>
        ) : (
          <Stack spacing={1.5}>
            <List dense disablePadding>
              {wallets.map(w => (
                <React.Fragment key={w.id}>
                  <ListItem disableGutters>
                    <ListItemText
                      primary={<Typography fontWeight={600}>{w.name}</Typography>}
                      secondary={w.currency}
                    />
                    <Typography fontWeight={700}>
                      {formatNumber(w.currentBalance)} {w.currency}
                    </Typography>
                  </ListItem>
                  <Divider component="li" />
                </React.Fragment>
              ))}
            </List>

            {/* Totals per currency */}
            <Stack spacing={0.5}>
              <Typography variant="caption" color="text.secondary">Totals</Typography>
              <Stack gap={1}>
                {Object.entries(totalsByCurrency).map(([ccy, amt]) => (
                  <Stack key={ccy} direction="row" justifyContent="space-between">
                    <Typography variant="body2">{ccy}</Typography>
                    <Typography variant="body2" fontWeight={700}>{formatNumber(amt)} {ccy}</Typography>
                  </Stack>
                ))}
              </Stack>
            </Stack>
          </Stack>
        )}
      </CardContent>

      {/* Modal */}
      <WalletCreateDialog open={open} onClose={() => setOpen(false)} onCreate={onCreate} loading={loading} />
    </Card>
  );
}
