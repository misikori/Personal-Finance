import type { Position, AllocationSlice } from "../../types/portfolio";

export function enrichPositions(raw: Position[]): Position[] {
  return raw.map(p => {
    const marketValue = +(p.qty * p.last).toFixed(2);
    const cost = p.qty * p.avgCost;
    const unrealizedAbs = +(marketValue - cost).toFixed(2);
    const unrealizedPct = cost > 0 ? +(unrealizedAbs / cost).toFixed(4) : 0;
    return { ...p, marketValue, unrealizedAbs, unrealizedPct };
  });
}

export function calcWeights(positions: Position[]) {
  const totalMV = positions.reduce((s, p) => s + p.marketValue, 0);
  const withWeights = positions.map(p => ({
    ...p,
    weightPct: totalMV > 0 ? (p.marketValue / totalMV) * 100 : 0,
  }));
  return { totalMV, positions: withWeights };
}

export function groupAllocation(
  positions: (Position & { weightPct?: number })[],
  by: "sector" | "class" = "sector"
): AllocationSlice[] {
  const map = new Map<string, number>();
  positions.forEach(p => {
    const key = (p as any)[by] as string;
    const v = (p.weightPct ?? 0);
    map.set(key, (map.get(key) ?? 0) + v);
  });
  // Normalize rounding to sum ~100
  const slices = Array.from(map.entries()).map(([label, value]) => ({ label, value }));
  const sum = slices.reduce((s, x) => s + x.value, 0);
  if (sum > 0 && Math.abs(sum - 100) > 0.01) {
    // adjust the largest slice to make 100%
    const idx = slices.reduce((m, _, i, arr) => arr[i].value > arr[m].value ? i : m, 0);
    slices[idx].value += (100 - sum);
  }
  return slices.sort((a,b) => b.value - a.value);
}
