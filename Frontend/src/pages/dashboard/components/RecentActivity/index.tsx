import { Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography, Chip } from "@mui/material";
import type { RecentTransaction } from "../../types";

type Props = { rows: RecentTransaction[] };

export default function RecentActivity({ rows }: Props) {
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
          {rows.map((r) => (
            <TableRow key={r.id} hover>
              <TableCell><Typography variant="body2">{new Date(r.ts).toLocaleString()}</Typography></TableCell>
              <TableCell>{r.symbol}</TableCell>
              <TableCell>
                <Chip
                  size="small"
                  label={r.side}
                  color={r.side === "BUY" ? "primary" : "success"}
                  variant="outlined"
                />
              </TableCell>
              <TableCell align="right">{r.qty}</TableCell>
              <TableCell align="right">{r.price.toFixed(2)}</TableCell>
              <TableCell align="right" sx={{ color: r.amount >= 0 ? "success.main" : "error.main" }}>
                {r.amount >= 0 ? "+" : "-"}${Math.abs(r.amount).toFixed(2)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
