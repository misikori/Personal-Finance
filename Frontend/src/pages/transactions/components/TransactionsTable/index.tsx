import { useMemo } from "react";
import {
  Box, Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Paper, TableSortLabel, Typography, Pagination, Stack, Select, MenuItem
} from "@mui/material";
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
import type { TransactionSortBy } from "../../../../domain/budget/types/transactionTypes";

export default function TransactionsTable({
  rows, total, page, pageSize, onPageChange, onPageSizeChange,
  sortBy, sortDir, onSortChange, loading
}: TransactionTableProps) {

  const pageCount = Math.max(1, Math.ceil(total / pageSize));
  const empty = !loading && total === 0;

  const header = useMemo(() => ([
    { id: "createdAt", label: "Date", sortable: true },
    { id: "type", label: "Type", sortable: false },
    { id: "categoryName", label: "Category", sortable: false },
    { id: "description", label: "Description", sortable: false },
    { id: "amount", label: "Amount", sortable: true, align: "right" as const },
    { id: "currency", label: "Currency", sortable: false, align: "right" as const },
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
              <TableRow key={String(r.id)} hover tabIndex={0} sx={focusableRowSx}>
                <TableCell>{fmtTime(r.date)}</TableCell>
                <TableCell>{r.type ?? "—"}</TableCell>
                <TableCell>{r.categoryName ?? "—"}</TableCell>
                <TableCell>{r.description ?? "—"}</TableCell>
                <TableCell align="right">
                  <Box component="span" sx={r.amount < 0 ? amountNegativeSx : amountPositiveSx}>
                    {fmtCurrency(r.amount, r.currency)}
                  </Box>
                </TableCell>
                <TableCell align="right">{r.currency}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

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
