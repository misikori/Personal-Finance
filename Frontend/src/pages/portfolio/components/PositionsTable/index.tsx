import {
  Paper, TableContainer, Table, TableHead, TableRow, TableCell, TableBody, Typography, Box
} from "@mui/material";
import type { Position } from "../../../../types/portfolio";
import { fmtCurrency, fmtPct } from "../../../../shared/utils/format";
import { containerSx, paperSx, footerRowSx, plPosSx, plNegSx, headerCellSx } from "./styles";

type Props = {
  rows: (Position & { weightPct: number })[];
};

export default function PositionsTable({ rows }: Props) {
  const totalMV = rows.reduce((s, r) => s + r.marketValue, 0);
  const totalWeight = rows.reduce((s, r) => s + r.weightPct, 0);

  return (
    <Paper variant="outlined" sx={paperSx}>
      <TableContainer sx={containerSx}>
        <Table size="small" aria-label="Positions">
          <TableHead>
            <TableRow>
              <TableCell component="th" scope="col" sx={headerCellSx}>Symbol</TableCell>
              <TableCell component="th" scope="col" sx={headerCellSx}>Name</TableCell>
              <TableCell component="th" scope="col" align="right" sx={headerCellSx}>Qty</TableCell>
              <TableCell component="th" scope="col" align="right" sx={headerCellSx}>Avg Cost</TableCell>
              <TableCell component="th" scope="col" align="right" sx={headerCellSx}>Last</TableCell>
              <TableCell component="th" scope="col" align="right" sx={headerCellSx}>Mkt Value</TableCell>
              <TableCell component="th" scope="col" align="right" sx={headerCellSx}>Unrealized P/L</TableCell>
              <TableCell component="th" scope="col" align="right" sx={headerCellSx}>Weight %</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.map(r => (
              <TableRow key={r.symbol} hover>
                <TableCell>{r.symbol}</TableCell>
                <TableCell>{r.name}</TableCell>
                <TableCell align="right">{r.qty}</TableCell>
                <TableCell align="right">{fmtCurrency(r.avgCost)}</TableCell>
                <TableCell align="right">{fmtCurrency(r.last)}</TableCell>
                <TableCell align="right">{fmtCurrency(r.marketValue)}</TableCell>
                <TableCell align="right">
                  <Box component="span" sx={r.unrealizedAbs < 0 ? plNegSx : plPosSx}>
                    {fmtCurrency(r.unrealizedAbs)} ({fmtPct(r.unrealizedPct * 100)})
                  </Box>
                </TableCell>
                <TableCell align="right">{(r.weightPct).toFixed(2)}%</TableCell>
              </TableRow>
            ))}

            {/* Totals row */}
            <TableRow sx={footerRowSx}>
              <TableCell colSpan={5}>
                <Typography variant="body2">Totals</Typography>
              </TableCell>
              <TableCell align="right">{fmtCurrency(totalMV)}</TableCell>
              <TableCell align="right">—</TableCell>
              <TableCell align="right">{totalWeight.toFixed(2)}%</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}
