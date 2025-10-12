import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import type { AllocationSlice, Position } from "../../../types/portfolio";
import { getCurrentUser } from "../../../auth/store/authStore";
import { marketService } from "../../../domain/portfolio/services/MarketDataService";
import { portfolioData } from "../../../domain/portfolio/services/PortfolioDataService";
import { TransactionDto } from "../../../domain/portfolio/types/transaction";

/** Try to read a last price from any likely shape of PriceQuoteDto */
function resolveLastPrice(q: any): number {
  // Common candidates: last, price, close, current, currentPrice, lastPrice
  return (
    q?.last ??
    q?.price ??
    q?.close ??
    q?.current ??
    q?.currentPrice ??
    q?.lastPrice ??
    0
  );
}

/** Normalize "Buy"/"Sell" regardless of backend casing/strings */
function parseSide(s: string | undefined): "Buy" | "Sell" | undefined {
  if (!s) return undefined;
  const v = String(s).toLowerCase();
  if (v.includes("sell")) return "Sell";
  if (v.includes("buy")) return "Buy";
  return undefined;
}

async function buildPositionsFromTransactions(
  txs: TransactionDto[]
): Promise<Position[]> {
  // Aggregate per symbol
  const by = new Map<
    string,
    {
      name?: string; // not in DTO; keep as symbol fallback
      qty: number;
      // Weighted average basis: sum(q * pricePerShare)
      costSum: number;
    }
  >();

  for (const t of txs) {
    const sym = t.symbol;
    if (!sym) continue;

    const side = parseSide(t.type);
    const signedQty = side === "Sell" ? -t.quantity : t.quantity;

    const node = by.get(sym) ?? { name: sym, qty: 0, costSum: 0 };
    node.qty += signedQty;
    node.costSum += signedQty * t.pricePerShare;
    by.set(sym, node);
  }

  // Fetch quotes concurrently
  const symbols = [...by.keys()];
  const quotes = await Promise.all(
    symbols.map(async (s) => {
      try {
        const q = await marketService.price(s);
        return [s, q] as const;
      } catch {
        return [s, null] as const;
      }
    })
  );
  const quoteMap = new Map<string, any>(quotes);

  // Build Position[]
  const positions: Position[] = [];
  for (const [symbol, agg] of by) {
    if (agg.qty === 0) continue; // ignore fully closed

    const last = resolveLastPrice(quoteMap.get(symbol));
    const avgCost = agg.qty !== 0 ? agg.costSum / agg.qty : 0;
    const marketValue = agg.qty * last;
    const unrealizedAbs = (last - avgCost) * agg.qty;
    const unrealizedPct = avgCost !== 0 ? (last - avgCost) / avgCost : 0;

    positions.push({
      symbol,
      name: agg.name ?? symbol,
      qty: agg.qty,
      avgCost,
      last,
      marketValue,
      unrealizedAbs,
      unrealizedPct,
      sector: "—", // not provided by DTOs; fill later if your API adds it
      class: "—",  // same here
    });
  }

  return positions;
}

export function usePortfolio() {
  const user = getCurrentUser();
  const username = user?.id ?? user?.email ?? "";

  // Load transactions (source of truth for positions)
  const transactionsQ = useQuery({
    queryKey: ["portfolio", "transactions", username],
    queryFn: () => portfolioData.transactions(username),
    enabled: !!username,
    staleTime: 60_000,
  });

  // Derive positions from transactions + live quotes
  const positionsQ = useQuery({
    queryKey: ["portfolio", "positions-from-transactions", username],
    enabled: !!username && !!transactionsQ.data,
    staleTime: 60_000,
    queryFn: async () => {
      const txs = (transactionsQ.data ?? []) as TransactionDto[];
      return await buildPositionsFromTransactions(txs);
    },
  });

  const positionsWithWeights = useMemo(() => {
    const list = (positionsQ.data ?? []) as Position[];
    const totalMV = list.reduce((s, p) => s + p.marketValue, 0) || 1;
    return list.map((p) => ({ ...p, weightPct: (p.marketValue / totalMV) * 100 }));
  }, [positionsQ.data]);

  const totalMV = useMemo(
    () => positionsWithWeights.reduce((s, p) => s + p.marketValue, 0),
    [positionsWithWeights]
  );

  // Compute allocation locally (by sector; you can switch to "class" if you start filling it)
  const allocation: AllocationSlice[] = useMemo(() => {
    const by = new Map<string, number>();
    for (const p of positionsWithWeights) {
      const key = p.sector || "Other";
      by.set(key, (by.get(key) ?? 0) + p.marketValue);
    }
    const entries = Array.from(by.entries());
    const sum = entries.reduce((s, [, v]) => s + v, 0) || 1;
    return entries
      .map(([label, mv]) => ({ label, value: (mv / sum) * 100 }))
      .sort((a, b) => b.value - a.value);
  }, [positionsWithWeights]);

  const loading = transactionsQ.isLoading || positionsQ.isLoading;

  return {
    positions: positionsWithWeights,
    allocation,
    totalMV,
    loading,
    error: transactionsQ.error || positionsQ.error,
    refetchAll: () => {
      transactionsQ.refetch();
      positionsQ.refetch();
    },
  };
}
