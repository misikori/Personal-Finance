import {
  Paper, TableContainer, Table, TableHead, TableRow, TableCell, TableBody, Typography
} from "@mui/material";
import { fmtCurrency, fmtPct } from "../../../../shared/utils/format";
import { containerSx, paperSx, footerRowSx, headerCellSx } from "./styles";
import { PositionDto } from "../../../../domain/portfolio/types/portfolio";

type Props = {
  rows: (PositionDto & { weightPct: number })[];
};

export default function PositionsTable({ rows }: Props) {
  const totalMV = rows.reduce((s, r) => s + r.currentValue, 0);
  const totalWeight = rows.reduce((s, r) => s + r.weightPct, 0);

  return (
    <Paper variant="outlined" sx={paperSx}>
      <TableContainer sx={containerSx}>
        <Table size="small" aria-label="Positions">
          <TableHead>
            <TableRow>
              <TableCell sx={headerCellSx}>Symbol</TableCell>
              <TableCell sx={headerCellSx}>Name</TableCell>
              <TableCell align="right" sx={headerCellSx}>Qty</TableCell>
              <TableCell align="right" sx={headerCellSx}>Avg Cost</TableCell>
              <TableCell align="right" sx={headerCellSx}>Last</TableCell>
              <TableCell align="right" sx={headerCellSx}>Mkt Value</TableCell>
              <TableCell align="right" sx={headerCellSx}>Gain/Loss</TableCell>
              <TableCell align="right" sx={headerCellSx}>Gain/Loss %</TableCell>
              <TableCell align="right" sx={headerCellSx}>Weight %</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map(r => (
              <TableRow key={r.symbol} hover>
                <TableCell>{r.symbol}</TableCell>
                <TableCell align="right">{r.quantity}</TableCell>
                <TableCell align="right">{fmtCurrency(r.averagePurchasePrice)}</TableCell>
                <TableCell align="right">{fmtCurrency(r.currentPrice)}</TableCell>
                <TableCell align="right">{fmtCurrency(r.currentValue)}</TableCell>
                <TableCell align="right">{fmtCurrency(r.gainLoss)}</TableCell>
                <TableCell align="right">{fmtPct(r.gainLossPercentage)}</TableCell>
                <TableCell align="right">{r.weightPct.toFixed(2)}%</TableCell>
              </TableRow>
            ))}

            <TableRow sx={footerRowSx}>
              <TableCell colSpan={5}>
                <Typography variant="body2">Totals</Typography>
              </TableCell>
              <TableCell align="right">{fmtCurrency(totalMV)}</TableCell>
              <TableCell align="right">—</TableCell>
              <TableCell align="right">—</TableCell>
              <TableCell align="right">{totalWeight.toFixed(2)}%</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}
