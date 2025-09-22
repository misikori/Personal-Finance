import { TransactionFilter } from "../../../../types/transaction";

export type TransactionFiltersProps = {
  value: TransactionFilter;
  onChange: (next: TransactionFilter) => void;
};