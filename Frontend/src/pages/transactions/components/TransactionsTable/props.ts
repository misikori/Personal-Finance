import { Transaction, TransactionFilter, TransactionSortBy } from "../../../../types/transaction";

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