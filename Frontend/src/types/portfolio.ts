export type Position = {
  symbol: string;
  name: string;
  qty: number;
  avgCost: number;
  last: number;
  marketValue: number;      
  unrealizedAbs: number;     
  unrealizedPct: number;  
  sector: string;         
  class: string;         
};

export type AllocationSlice = {
  label: string; 
  value: number;  
};