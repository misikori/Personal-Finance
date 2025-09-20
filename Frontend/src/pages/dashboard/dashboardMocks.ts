import { Kpi, RecentTransaction, TopMover } from "./types";

const fmtMoney = (n: number, currency = "USD") =>
  new Intl.NumberFormat(undefined, { style: "currency", currency, maximumFractionDigits: 0 }).format(n);

const fmtSignedMoney = (n: number, currency = "USD") => {
  const sign = n >= 0 ? "+" : "-";
  return `${sign}${fmtMoney(Math.abs(n), currency)}`;
};

export async function getKpisMock(): Promise<Kpi[]> {
  return [
    { id: "totalBalance", label: "Total Balance", value: fmtMoney(128_450) },
    { id: "dailyPL", label: "Daily P/L", value: fmtSignedMoney(1_320), trend: "up", sublabel: "vs. prev close" },
    { id: "mtdReturn", label: "MTD Return %", value: "+2.8%", trend: "up" },
    { id: "cash", label: "Cash", value: fmtMoney(18_900) },
  ];
}

export async function getRecentTransactionsMock(): Promise<RecentTransaction[]> {
  const now = Date.now();
  const mk = (i: number, sym: string, side: "BUY" | "SELL", qty: number, price: number): RecentTransaction => ({
    id: `${now}-${i}`,
    ts: new Date(now - i * 36e5).toISOString(),
    symbol: sym,
    side,
    qty,
    price,
    amount: side === "BUY" ? -(qty * price) : +(qty * price),
  });

  return [
    mk(0, "MSFT", "BUY", 5, 420.1),
    mk(1, "AAPL", "SELL", 3, 232.3),
    mk(2, "NVDA", "BUY", 1, 115.9),
    mk(3, "XOM" , "BUY", 8, 118.2),
    mk(4, "SPY" , "SELL", 2, 557.0),
  ];
}

export async function getTopMoversMock(): Promise<TopMover[]> {
  return [
    { symbol: "NVDA", name: "NVIDIA",       changePct: +3.6 },
    { symbol: "AMD",  name: "AMD",          changePct: +2.4 },
    { symbol: "AAPL", name: "Apple",        changePct: -1.1 },
    { symbol: "XOM",  name: "Exxon Mobil",  changePct: +0.9 },
    { symbol: "TSLA", name: "Tesla",        changePct: -0.6 },
  ];
}
