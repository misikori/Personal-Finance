import { useEffect, useState } from "react";
import type { AllocationSlice, Position } from "../../../types/portfolio";
import { getAllocation, getPositions } from "../../../services/portfolioService";

export function usePortfolio() {
  const [positions, setPositions] = useState<(Position & { weightPct: number })[]>([]);
  const [allocation, setAllocation] = useState<AllocationSlice[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let ok = true;
    (async () => {
      setLoading(true);
      const [pos, alloc] = await Promise.all([getPositions(), getAllocation("sector")]);
      if (!ok) return;
      setPositions(pos);
      setAllocation(alloc);
      setLoading(false);
    })();
    return () => { ok = false; };
  }, []);

  const totalMV = positions.reduce((s, p) => s + p.marketValue, 0);

  return { positions, allocation, totalMV, loading };
}
