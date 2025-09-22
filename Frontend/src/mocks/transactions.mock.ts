import type { Transaction, TransactionType } from "../types/transaction";

const ACCOUNTS = ["Brokerage", "Retirement", "Savings"] as const;
const TYPES: TransactionType[] = ["BUY","SELL","DIV","DEPOSIT","WITHDRAWAL","FEE"];
const SYMBOLS = ["AAPL","MSFT","NVDA","GOOGL","AMZN","TSLA","XOM","JNJ","KO","MA","V","META"];

function rand(seed: number) {
  // tiny deterministic LCG so snapshots stay stable
  let s = seed % 2147483647;
  return () => (s = s * 48271 % 2147483647) / 2147483647;
}

const rng = rand(42);
function pick<T>(arr: readonly T[]) { return arr[Math.floor(rng()*arr.length)]; }

function daysAgoISO(n: number) {
  const d = new Date();
  d.setDate(d.getDate() - n);
  d.setHours(12,0,0,0);
  return d.toISOString();
}

export const TRANSACTIONS_MOCK: Transaction[] = Array.from({ length: 120 }).map((_, i) => {
  const t = pick(TYPES);
  const sym = t === "DEPOSIT" || t === "WITHDRAWAL" ? undefined : pick(SYMBOLS);
  const qty = t === "BUY" || t === "SELL" ? Math.max(1, Math.round(rng()*50)) : undefined;
  const price = t === "BUY" || t === "SELL" ? +(50 + rng()*400).toFixed(2) : undefined;
  const fees = t === "BUY" || t === "SELL" ? +(rng()*5).toFixed(2) : (t === "FEE" ? +(rng()*4+1).toFixed(2) : 0);
  let amount = 0;
  if (t === "BUY" && qty && price) amount = -(qty*price + (fees||0));
  if (t === "SELL" && qty && price) amount = +(qty*price - (fees||0));
  if (t === "DIV") amount = +(rng()*50+5).toFixed(2);
  if (t === "DEPOSIT") amount = +(rng()*2000+200).toFixed(2);
  if (t === "WITHDRAWAL") amount = -+(rng()*1500+100).toFixed(2);
  if (t === "FEE") amount = -(fees || 0);

  return {
    id: `tx-${i+1}`,
    ts: daysAgoISO(Math.floor(i * 0.7)), // spread across ~3 months
    type: t,
    symbol: sym,
    qty,
    price,
    fees,
    amount: +amount.toFixed(2),
    account: pick(ACCOUNTS as unknown as string[]),
    currency: "USD",
  } as Transaction;
});

export const TRANSACTION_TYPES = TYPES;
export const TRANSACTION_ACCOUNTS = Array.from(new Set(TRANSACTIONS_MOCK.map(t => t.account)));
