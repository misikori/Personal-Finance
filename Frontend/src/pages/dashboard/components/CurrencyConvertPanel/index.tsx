import * as React from "react";
import { Card, CardContent, CardHeader, Typography, Stack, Button, TextField, MenuItem } from "@mui/material";
import { convertPanelStyles } from "./style";
import type { CurrencyConvertPanelProps } from "./types";
import { useCurrencies, useCurrencyConvert } from "../../../../domain/currency/hooks/useCurrency";

export function CurrencyConvertPanel(props: CurrencyConvertPanelProps) {
  const { currencies } = useCurrencies("USD");
  const { result, loading, error, convert } = useCurrencyConvert();
  const [from, setFrom] = React.useState("");
  const [to, setTo] = React.useState("");
  const [amount, setAmount] = React.useState(0);

  const handleConvert = () => {
    if (!from || !to || !amount) return;
    convert({ from, to, amount });
  };

  return (
    <Card variant="outlined" sx={convertPanelStyles.root}>
      <CardHeader title={<Typography variant="subtitle1" fontWeight={700}>Convert Currency</Typography>} />
      <CardContent>
        <Stack spacing={2}>
          <TextField
            select
            label="From"
            value={from}
            onChange={e => setFrom(e.target.value)}
            fullWidth
          >
            {currencies.map(c => (
              <MenuItem key={c.code} value={c.code}>{c.code}</MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="To"
            value={to}
            onChange={e => setTo(e.target.value)}
            fullWidth
          >
            {currencies.map(c => (
              <MenuItem key={c.code} value={c.code}>{c.code}</MenuItem>
            ))}
          </TextField>
          <TextField
            label="Amount"
            type="number"
            value={amount}
            onChange={e => setAmount(Number(e.target.value))}
            fullWidth
          />
          <Button variant="contained" onClick={handleConvert} disabled={loading}>
            Convert
          </Button>
          {error && <Typography color="error">{error}</Typography>}
          {result && (
            <Typography>
              {result.amount} {result.from} = {result.converted} {result.to} (Rate: {result.rate})
            </Typography>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}
