import { useMemo } from "react";
import {
  Box, Stack, TextField, InputLabel, MenuItem, Select, FormControl, OutlinedInput
} from "@mui/material";
import { formRootSx, rowStackSx } from "./styles";
import { useWallets } from "../../hooks/useWallets";
import type { TransactionFilter, TransactionType } from "../../../../domain/budget/types/transactionTypes";
import { TransactionFiltersProps } from "./props";

const TYPE_OPTIONS: (TransactionType | "All")[] = ["All", "Income", "Expense"];

export default function TransactionFilters({ value, onChange }: TransactionFiltersProps) {
  const { wallets } = useWallets();

  const handle = (patch: Partial<TransactionFilter>) => onChange({ ...value, ...patch });

  const walletMenu = useMemo(
    () => wallets.map(w => <MenuItem key={w.id} value={w.id}>{w.name} ({w.currency})</MenuItem>),
    [wallets]
  );

  return (
    <Box component="form" noValidate sx={formRootSx}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={rowStackSx}>
        <FormControl sx={{ minWidth: 220 }}>
          <InputLabel id="wallet-label">Wallet</InputLabel>
          <Select
            labelId="wallet-label"
            input={<OutlinedInput label="Wallet" />}
            value={value.walletId ?? ""}
            onChange={(e) => handle({ walletId: String(e.target.value), page: 1 })}
          >
            {walletMenu}
          </Select>
        </FormControl>

        <TextField
          className="date-from"
          label="From"
          type="date"
          InputLabelProps={{ shrink: true }}
          value={value.dateFrom ?? ""}
          onChange={(e) => handle({ dateFrom: e.target.value || undefined, page: 1 })}
        />
        <TextField
          className="date-to"
          label="To"
          type="date"
          InputLabelProps={{ shrink: true }}
          value={value.dateTo ?? ""}
          onChange={(e) => handle({ dateTo: e.target.value || undefined, page: 1 })}
        />

        <TextField
          label="Category"
          placeholder="e.g. Groceries"
          value={value.categoryName ?? ""}
          onChange={(e) => handle({ categoryName: e.target.value || undefined, page: 1 })}
        />

        <FormControl sx={{ minWidth: 160 }}>
          <InputLabel id="type-label">Type</InputLabel>
          <Select
            labelId="type-label"
            input={<OutlinedInput label="Type" />}
            value={value.type ?? "All"}
            onChange={(e) => {
              const v = e.target.value as TransactionType | "All";
              handle({ type: v === "All" ? undefined : v, page: 1 });
            }}
          >
            {TYPE_OPTIONS.map(t => <MenuItem key={t} value={t}>{t}</MenuItem>)}
          </Select>
        </FormControl>
      </Stack>
    </Box>
  );
}
