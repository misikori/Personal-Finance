export type Kpi = {
  id: 'totalBalance' | 'dailyPL' | 'mtdReturn' | 'cash';
  label: string;
  value: string;       
  sublabel?: string;    
  trend?: 'up' | 'down' | 'flat';
};

export type RecentTransaction = {
  id: string;
  ts: string;       
  symbol: string;
  side: 'BUY' | 'SELL';
  qty: number;
  price: number;
  amount: number;      
};

export type TopMover = {
  symbol: string;
  name?: string;
  changePct: number;    
};