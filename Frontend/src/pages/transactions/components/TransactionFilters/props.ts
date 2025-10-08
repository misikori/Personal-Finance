import { TransactionFilter } from "../../../../domain/budget/types/transactionTypes";


export type TransactionFiltersProps = {
  value: TransactionFilter;
  onChange: (next: TransactionFilter) => void;
};