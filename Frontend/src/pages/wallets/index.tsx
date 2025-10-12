import {
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Box,
  Chip,
  Stack,
  Typography,
  List,
  ListItem,
  ListItemText,
  Divider
} from "@mui/material";
import { GenerateReportButton } from "./components/GenerateReportButton";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { useEffect, useState } from "react";
import type { Transaction } from "../../domain/budget/types/transactionTypes";
import { fmtCurrency, fmtTime } from "../../shared/utils/format";
import { useWallets } from "../transactions/hooks/useWallets";
import { BudgetService } from "../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../auth/store/authStore";

export default function WalletsPage() {
  const { wallets, loading: walletsLoading } = useWallets();
  const [byWallet, setByWallet] = useState<Record<string, Transaction[]>>({});
  
  const currentUser = getCurrentUser();
  useEffect(() => {
    let alive = true;

    (async () => {
      if (!wallets.length) {
        if (alive) setByWallet({});
        return;
      }

      const entries = await Promise.all(
        wallets.map(async (w) => {
          const txs = await BudgetService.transactions.list(w.id);
          return [String(w.id), txs] as const;
        })
      );

      if (alive) setByWallet(Object.fromEntries(entries));
    })();

    return () => {
      alive = false;
    };
  }, [wallets]);

  if (walletsLoading) {
    return <Typography>Loading wallets…</Typography>;
  }

  return (
    <Box sx={{ width: "50vw", mx: "auto", py: 2 }}>

      {/* ---------- Page Header ---------- */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 3 }}>
        <Typography variant="h4" fontWeight={600}>Wallets</Typography>
      </Stack>

      {/* ---------- Generate Report Button ---------- */}
      <Box sx={{ mb: 2 }}>
        <GenerateReportButton
          userId={currentUser?.id || ""}
          username={currentUser?.email || ""}
          emailAddress={currentUser?.email || ""}
        />
      </Box>

      {/* ---------- Wallets Accordion List ---------- */}
      <Stack spacing={1.5}>
        {wallets.map((w) => {
          const items = byWallet[w.id] ?? [];
          const balance = items.reduce(
            (acc, t) => acc + (t.type === "Expense" ? -t.amount : t.amount),
            0
          );

          return (
            <Accordion
              key={String(w.id)}
              disableGutters
              sx={{
                borderRadius: 2,
                "&:before": { display: "none" },
                border: "1px solid",
                borderColor: "divider",
                boxShadow: 1,
              }}
            >
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Stack direction="row" spacing={2} alignItems="center" sx={{ width: "100%", pr: 1 }}>
                  <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 600 }}>
                    {w.name}
                  </Typography>

                  <Chip
                    size="small"
                    label={`Balance: ${fmtCurrency(balance, w.currency)}`}
                    sx={{
                      fontWeight: 600,
                      bgcolor: (theme) => theme.palette.mode === "dark" ? "grey.800" : "grey.100",
                    }}
                  />
                  <Chip size="small" label={w.currency} />
                  <Chip size="small" label={`${items.length} tx`} />
                </Stack>
              </AccordionSummary>

              <AccordionDetails
                sx={{
                  bgcolor: (theme) => theme.palette.mode === "dark" ? "grey.900" : "grey.50",
                  px: 3,
                  py: 2,
                }}
              >
                {items.length === 0 ? (
                  <Typography color="text.secondary">No transactions in this wallet yet.</Typography>
                ) : (
                  <List dense disablePadding>
                    {items.map((t, idx) => {
                      const isExpense = t.type === "Expense";
                      const color = (theme: any) =>
                        isExpense ? theme.palette.error.main : theme.palette.success.main;

                      return (
                        <Box key={String(t.id)}>
                          <ListItem
                            secondaryAction={
                              <Typography
                                component="span"       // <-- avoid <p> in secondaryAction
                                sx={{ fontWeight: 700, color, minWidth: 110, textAlign: "right" }}
                              >
                                {isExpense ? "- " : ""}
                                {fmtCurrency(t.amount, t.currency)}
                              </Typography>
                            }
                          >
                            <ListItemText
                              // Render the wrapper as a div to prevent <p> nesting
                              primaryTypographyProps={{ variant: "body2", component: "div" }}
                              // We don’t use secondary here, but keep it safe if used later
                              secondaryTypographyProps={{ component: "span" }}
                              primary={
                                <Stack direction="row" spacing={1.5} flexWrap="wrap" alignItems="center">
                                  <Typography variant="body2" component="time" sx={{ minWidth: 100 }}>
                                    {fmtTime((t as any).createdAt ?? (t as any).date)}
                                  </Typography>
                                  <Typography variant="body2" component="span" sx={{ opacity: 0.7 }}>
                                    {t.type ?? "—"}
                                  </Typography>
                                  <Typography variant="body2" component="span">•</Typography>
                                  <Typography variant="body2" component="span">
                                    {t.categoryName ?? "—"}
                                  </Typography>
                                  {t.description && (
                                    <>
                                      <Typography variant="body2" component="span">•</Typography>
                                      <Typography variant="body2" component="span" sx={{ opacity: 0.8 }}>
                                        {t.description}
                                      </Typography>
                                    </>
                                  )}
                                </Stack>
                              }
                            />
                          </ListItem>
                          {idx < items.length - 1 && <Divider component="li" />}
                        </Box>
                      );
                    })}
                  </List>
                )}
              </AccordionDetails>
            </Accordion>
          );
        })}
      </Stack>
    </Box>
  );
}
