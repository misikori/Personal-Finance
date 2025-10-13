import { useEffect, useMemo, useState } from "react";
import {
  Box, Stack, Typography, Button, Card, CardHeader, CardContent,
  Chip, IconButton, Tooltip, Skeleton, Divider, Menu, MenuItem
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import RefreshIcon from "@mui/icons-material/Refresh";
import RepeatIcon from "@mui/icons-material/Repeat";
import EventRepeatIcon from "@mui/icons-material/EventRepeat";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import PauseIcon from "@mui/icons-material/Pause";
import SkipNextIcon from "@mui/icons-material/SkipNext";
import DeleteIcon from "@mui/icons-material/Delete";
import { BudgetService } from "../../domain/budget/services/BudgetService";
import { getCurrentUser } from "../../auth/store/authStore";
import type { RecurringTransaction } from "../../domain/budget/types/budgetServiceTypes";
import RecurringTransactionCreateDialog from "./components/RecurringTransactionCreateDialog";

type ActionAnchor = { id: string; anchorEl: HTMLElement | null };
export enum TransactionTypeEnum {
  Expense = 1,
  Income = 2
}


export default function RecurringTransactionsPage() {
  const [rows, setRows] = useState<RecurringTransaction[]>([]);
  const [loading, setLoading] = useState(false);
  const [openNew, setOpenNew] = useState(false);
  const [menu, setMenu] = useState<ActionAnchor | null>(null);
  const userId = getCurrentUser()?.id ?? null;

  const load = async () => {
    if (!userId) return;
    setLoading(true);
    try {
      const res = await BudgetService.recurring.getByUser(userId);
      setRows(res ?? []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [userId]);

  const handleOpenMenu = (id: string) => (e: React.MouseEvent<HTMLElement>) =>
    setMenu({ id, anchorEl: e.currentTarget });
  const handleCloseMenu = () => setMenu(null);

  const sorted = useMemo(
    () =>
      [...rows].sort((a, b) =>
        new Date(a.nextDueDate).getTime() - new Date(b.nextDueDate).getTime()
      ),
    [rows]
  );

  return (
    <Stack spacing={3} width={"100%"} >
      <Stack direction="row" alignItems="center" justifyContent="space-between">
        <Typography variant="h5" fontWeight={600}>
          Recurring Transactions
        </Typography>
        <Stack direction="row" spacing={1}>
          <Tooltip title="Refresh">
            <span>
              <IconButton onClick={load} disabled={loading}>
                <RefreshIcon />
              </IconButton>
            </span>
          </Tooltip>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setOpenNew(true)}>
            Add Recurring
          </Button>
        </Stack>
      </Stack>

      {loading ? (
        <Box  display="flex" flexWrap="wrap" gap={2}>
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton
              key={i}
              variant="rectangular"
              width={300}
              height={160}
              sx={{ borderRadius: 2 }}
            />
          ))}
        </Box>
      ) : rows.length === 0 ? (
        <EmptyState onAdd={() => setOpenNew(true)} />
      ) : (
        <Box
          display="flex"
          flexWrap="wrap"
          justifyContent="flex-start"
          gap={2}
          width="100%"
        >
          {sorted.map((r) => (
            <Box key={String(r.id)} sx={{
        minWidth: 300,             
        maxWidth: 500,              
        '@media (max-width: 600px)': {
          flex: '1 1 100%',        
        },
      }}>
              <RecurringCard row={r} onMenu={handleOpenMenu(String(r.id))} />
            </Box>
          ))}
        </Box>
      )}

      <Menu
        open={!!menu?.anchorEl}
        anchorEl={menu?.anchorEl}
        onClose={handleCloseMenu}
      >
        <MenuItem>
          <PauseIcon fontSize="small" sx={{ mr: 1 }} /> Pause / Resume
        </MenuItem>
        <MenuItem>
          <SkipNextIcon fontSize="small" sx={{ mr: 1 }} /> Skip next
        </MenuItem>
        <Divider />
        <MenuItem>
          <DeleteIcon fontSize="small" sx={{ mr: 1 }} /> Delete
        </MenuItem>
      </Menu>

      <RecurringTransactionCreateDialog
        open={openNew}
        onClose={() => setOpenNew(false)}
        onCreated={load}
      />
    </Stack>
  );
}

/* ---------- Card ---------- */

function RecurringCard({
  row,
  onMenu,
}: {
  row: RecurringTransaction;
  onMenu: (e: React.MouseEvent<HTMLElement>) => void;
}) {
  const {
    amount,
    currency,
    transactionType,
    category,
    description,
    frequency,
    nextDueDate: nextRun,
    startDate,
    endDate,
  } = row;
  const isExpense = transactionType === "Expense";

  return (
    <Card variant="outlined" sx={{ height: "100%", borderRadius: 2 }}>
      <CardHeader
        title={
          <Stack direction="row" spacing={1} alignItems="center">
            <RepeatIcon fontSize="small" />
            <Typography variant="subtitle1" fontWeight={600}>
              {category || "Uncategorized"}
            </Typography>
            <Chip
              size="small"
              label={humanFrequency(frequency)}
              icon={<EventRepeatIcon />}
              variant="outlined"
            />
          </Stack>
        }
        subheader={description || "—"}
        action={
          <IconButton onClick={onMenu}>
            <MoreVertIcon />
          </IconButton>
        }
      />
      <CardContent sx={{ pt: 0 }}>
        <Stack spacing={1}>
          <Typography fontWeight={600} color={isExpense ? "error.main" : "success.main"}>
            {isExpense ? "−" : "+"} {amount.toFixed(2)} {currency}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Next run: {nextRun ? new Date(nextRun).toLocaleString() : "—"}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Active: {fmtDate(startDate)} → {fmtDate(endDate)}
          </Typography>
        </Stack>
      </CardContent>
    </Card>
  );
}

/* ---------- Empty State ---------- */

function EmptyState({ onAdd }: { onAdd: () => void }) {
  return (
    <Box
      sx={{
        p: 5,
        border: "1px dashed",
        borderColor: "divider",
        borderRadius: 2,
        textAlign: "center",
      }}
    >
      <Typography variant="h6" gutterBottom>
        No recurring transactions yet
      </Typography>
      <Typography color="text.secondary" sx={{ mb: 2 }}>
        Automate rent, subscriptions, or recurring expenses easily.
      </Typography>
      <Button variant="contained" startIcon={<AddIcon />} onClick={onAdd}>
        Add Recurring Transaction
      </Button>
    </Box>
  );
}

/* ---------- Helpers ---------- */

function fmtDate(iso?: string) {
  if (!iso) return "—";
  const d = new Date(iso);
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "2-digit" });
}

function humanFrequency(freq: RecurringTransaction["frequency"]) {
  switch (freq) {
    case "Weekly": return "Weekly";
    case "Monthly": return "Monthly";
    case "Yearly": return "Yearly";
    default: return String(freq ?? "Custom");
  }
}
