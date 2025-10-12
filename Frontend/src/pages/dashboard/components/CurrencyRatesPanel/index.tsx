import * as React from "react";
import { Card, CardContent, CardHeader, Typography, Stack, Divider } from "@mui/material";

import { ratesPanelStyles } from "./style";
import type { CurrencyRatesPanelProps } from "./types";
import { useCurrencies } from "../../../../domain/currency/hooks/useCurrency";


import { TextField, MenuItem, InputAdornment, IconButton } from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";

export function CurrencyRatesPanel(props: CurrencyRatesPanelProps) {
  const [baseCurrency, setBaseCurrency] = React.useState("RSD");
  const [search, setSearch] = React.useState("");
  const { currencies, loading, error } = useCurrencies(baseCurrency);

  // Filter and show only 5 rates
  const filtered = React.useMemo(() => {
    let arr = currencies;
    if (search) {
      arr = arr.filter(c => c.code.toLowerCase().includes(search.toLowerCase()));
    }
    return arr.slice(0, 5);
  }, [currencies, search]);

  // Get all currency codes for base selection
  const allCodes = React.useMemo(() => currencies.map(c => c.code), [currencies]);

  return (
    <Card variant="outlined" sx={ratesPanelStyles.root}>
      <CardHeader title={<Typography variant="subtitle1" fontWeight={700}>Currency Rates</Typography>} />
      <CardContent>
        <Stack spacing={2}>
          <TextField
            select
            label="Base Currency"
            value={baseCurrency}
            onChange={e => setBaseCurrency(e.target.value)}
            size="small"
            sx={{ maxWidth: 180 }}
          >
            {allCodes.map(code => (
              <MenuItem key={code} value={code}>{code}</MenuItem>
            ))}
          </TextField>
          <TextField
            label="Search rates"
            value={search}
            onChange={e => setSearch(e.target.value)}
            size="small"
            InputProps={{
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton edge="end" size="small">
                    <SearchIcon />
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
          {loading ? (
            <Typography>Loading...</Typography>
          ) : error ? (
            <Typography color="error">{error}</Typography>
          ) : (
            <Stack spacing={1.5}>
              {filtered.map(c => (
                <React.Fragment key={c.code}>
                  <Stack direction="row" justifyContent="space-between">
                    <Typography fontWeight={600}>{c.code}</Typography>
                    <Typography>{c.exchangeMiddle}</Typography>
                  </Stack>
                  <Divider />
                </React.Fragment>
              ))}
            </Stack>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}
