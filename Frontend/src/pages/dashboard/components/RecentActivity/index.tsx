import {
  Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography, Chip, Box
} from "@mui/material";
import type { RecentTransaction } from "../../types";

const fmtMoney = (n: number) => n.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const fmtDateTime = (iso: string) => new Date(iso).toLocaleString();

type Props = { rows: RecentTransaction[] };

export default function RecentActivity({ rows }: Props) {
  if (!rows?.length) {
    return (
      <Paper variant="outlined" sx={{ borderRadius: 3, p: 1.5 }}>
        <Typography variant="body2" color="text.secondary">No recent activity.</Typography>
      </Paper>
    );
  }

  return (
    <TableContainer component={Paper} sx={{ borderRadius: 3 }}>
      <Table size="small" aria-label="Recent Activity">
        <TableHead>
          <TableRow>
            <TableCell>Date</TableCell>
            <TableCell>Symbol</TableCell>
            <TableCell>Side</TableCell>
            <TableCell align="right">Qty</TableCell>
            <TableCell align="right">Price</TableCell>
            <TableCell align="right">Amount</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((r) => {
            // Signed amount: BUY = cash outflow (negative), SELL = inflow (positive)
            const signedAmount = r.side === "BUY" ? -Math.abs(r.amount) : Math.abs(r.amount);
            const amountColor = signedAmount >= 0 ? "success.main" : "error.main";
            const chipColor = r.side === "BUY" ? "success" : "error";

            return (
              <TableRow key={r.id} hover>
                <TableCell><Typography variant="body2">{fmtDateTime(r.ts)}</Typography></TableCell>
                <TableCell>{r.symbol}</TableCell>
                <TableCell>
                  <Chip size="small" label={r.side} color={chipColor} variant="outlined" />
                </TableCell>
                <TableCell align="right">{r.qty}</TableCell>
                <TableCell align="right">{fmtMoney(r.price)}</TableCell>
                <TableCell align="right" sx={{ color: amountColor }}>
                  <Box component="span" sx={{ fontVariantNumeric: "tabular-nums" }}>
                    {signedAmount >= 0 ? "+" : "-"}${fmtMoney(Math.abs(signedAmount))}
                  </Box>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
