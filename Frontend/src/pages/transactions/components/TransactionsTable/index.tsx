import { useMemo } from "react";
import {
  Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, TableSortLabel, Typography, Pagination, Stack, Select, MenuItem
} from "@mui/material";
import { TransactionSortBy } from "../../../../types/transaction";
import { TransactionTableProps } from "./props";
import { fmtCurrency, fmtTime } from "../../../../shared/utils/format";
import {
  tablePaperSx,
  tableContainerSx,
  focusableRowSx,
  paginationBarSx,
  amountPositiveSx,
  amountNegativeSx,
} from "./styles";

export default function TransactionsTable({
  rows, total, page, pageSize, onPageChange, onPageSizeChange,
  sortBy, sortDir, onSortChange, loading
}: TransactionTableProps) {

  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  const empty = !loading && total === 0;

  const header = useMemo(() => ([
    { id: "ts", label: "Date", sortable: true },
    { id: "type", label: "Type", sortable: false },
    { id: "symbol", label: "Symbol", sortable: false },
    { id: "qty", label: "Qty", sortable: false, align: "right" as const },
    { id: "price", label: "Price", sortable: false, align: "right" as const },
    { id: "fees", label: "Fees", sortable: false, align: "right" as const },
    { id: "amount", label: "Amount", sortable: true, align: "right" as const },
    { id: "account", label: "Account", sortable: false },
  ]), []);

  return (
    <Paper variant="outlined" sx={tablePaperSx}>
      <TableContainer sx={tableContainerSx}>
        <Table stickyHeader size="small" aria-label="All transactions">
          <TableHead>
            <TableRow>
              {header.map(col => (
                <TableCell key={col.id} align={col.align}>
                  {col.sortable ? (
                    <TableSortLabel
                      active={sortBy === col.id}
                      direction={sortBy === col.id ? sortDir : "asc"}
                      onClick={() => onSortChange(col.id as TransactionSortBy)}
                    >
                      {col.label}
                    </TableSortLabel>
                  ) : col.label}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>

          <TableBody>
            {loading && (
              <TableRow>
                <TableCell colSpan={header.length}>
                  <Typography variant="body2" color="text.secondary">Loading…</Typography>
                </TableCell>
              </TableRow>
            )}

            {empty && (
              <TableRow>
                <TableCell colSpan={header.length}>
                  <Typography variant="body2" color="text.secondary">
                    No results. Try adjusting filters.
                  </Typography>
                </TableCell>
              </TableRow>
            )}

            {!loading && rows.map((r) => (
              <TableRow
                key={r.id}
                hover
                tabIndex={0}
                sx={focusableRowSx}
              >
                <TableCell>{fmtTime(r.ts)}</TableCell>
                <TableCell>{r.type}</TableCell>
                <TableCell>{r.symbol ?? "—"}</TableCell>
                <TableCell align="right">{r.qty ?? "—"}</TableCell>
                <TableCell align="right">{r.price ? fmtCurrency(r.price, r.currency ?? "USD") : "—"}</TableCell>
                <TableCell align="right">{typeof r.fees === "number" ? fmtCurrency(r.fees, r.currency ?? "USD") : "—"}</TableCell>
                <TableCell align="right">
                  <Box component="span" sx={r.amount < 0 ? amountNegativeSx : amountPositiveSx}>
                    {fmtCurrency(r.amount, r.currency ?? "USD")}
                  </Box>
                </TableCell>
                <TableCell>{r.account}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Pagination controls */}
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={paginationBarSx}>
        <Typography variant="caption" color="text.secondary">
          {total.toLocaleString()} total
        </Typography>

        <Stack direction="row" spacing={2} alignItems="center">
          <Select
            size="small"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
          >
            {[10, 25, 50].map(ps => <MenuItem key={ps} value={ps}>{ps}/page</MenuItem>)}
          </Select>

          <Pagination
            count={pageCount}
            page={Math.min(page, pageCount)}
            onChange={(_, p) => onPageChange(p)}
            shape="rounded"
            color="primary"
            siblingCount={0}
            boundaryCount={1}
          />
        </Stack>
      </Stack>
    </Paper>
  );
}
