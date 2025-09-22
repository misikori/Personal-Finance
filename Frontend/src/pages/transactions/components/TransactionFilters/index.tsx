import { useMemo } from "react";
import { Box, Stack, TextField, InputLabel, MenuItem, Select, FormControl, OutlinedInput, Checkbox, ListItemText } from "@mui/material";
import { TransactionFiltersProps } from "./props";
import { TRANSACTION_ACCOUNTS, TRANSACTION_TYPES } from "../../../../mocks/transactions.mock";
import { TransactionFilter, TransactionType } from "../../../../types/transaction";
import { formRootSx, rowStackSx } from "./styles";

export default function TransactionFilters({ value, onChange }: TransactionFiltersProps) {
  const accounts = TRANSACTION_ACCOUNTS;
  const types = TRANSACTION_TYPES;

  const selectedTypes = value.types ?? [];

  const handle = (patch: Partial<TransactionFilter>) => onChange({ ...value, ...patch });

  const typeMenuItems = useMemo(() => (
    types.map((t) => (
      <MenuItem key={t} value={t}>
        <Checkbox checked={selectedTypes.includes(t)} />
        <ListItemText primary={t} />
      </MenuItem>
    ))
  ), [types, selectedTypes]);

  return (
    <Box component="form" noValidate sx={formRootSx}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2} sx={rowStackSx}>
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
        <FormControl className="types-select">
          <InputLabel id="types-label">Types</InputLabel>
          <Select
            labelId="types-label"
            multiple
            input={<OutlinedInput label="Types" />}
            value={selectedTypes}
            onChange={(e) => handle({ types: e.target.value as TransactionType[], page: 1 })}
            renderValue={(sel) => (sel as string[]).join(", ")}
          >
            {typeMenuItems}
          </Select>
        </FormControl>
        <TextField
          label="Symbol"
          placeholder="e.g. AAPL"
          value={value.symbolContains ?? ""}
          onChange={(e) => handle({ symbolContains: e.target.value || undefined, page: 1 })}
          inputProps={{ maxLength: 10 }}
        />
        <FormControl className="account-select">
          <InputLabel id="account-label">Account</InputLabel>
          <Select
            labelId="account-label"
            label="Account"
            value={value.account ?? ""}
            onChange={(e) => handle({ account: (e.target.value || undefined) as string | undefined, page: 1 })}
          >
            <MenuItem value=""><em>Any</em></MenuItem>
            {accounts.map(a => <MenuItem key={a} value={a}>{a}</MenuItem>)}
          </Select>
        </FormControl>
      </Stack>
    </Box>
  );
}
