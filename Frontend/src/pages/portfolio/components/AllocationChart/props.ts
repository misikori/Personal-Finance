import type { AllocationSlice } from "../../../../types/portfolio";

export type AllocationChartProps = {
  data: AllocationSlice[];
  title?: string;
  legend?: boolean;       
  by?: "sector" | "class"; 
};
