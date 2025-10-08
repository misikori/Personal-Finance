import { Transaction, TransactionSortBy, TransactionFilter } from "../../../../domain/budget/types/transactionTypes";

export type TransactionTableProps = {
  rows: Transaction[];
  total: number;
  page: number;          
  pageSize: number;
  onPageChange: (p: number) => void;
  onPageSizeChange: (ps: number) => void;
  sortBy: TransactionSortBy;
  sortDir: "asc" | "desc";
  onSortChange: (by: TransactionSortBy) => void;
  loading?: boolean;
  filter: TransactionFilter;
};