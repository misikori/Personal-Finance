import { POSITIONS_MOCK } from "../mocks/portfolio.mock";
import type { AllocationSlice, Position } from "../types/portfolio";
import { enrichPositions, calcWeights, groupAllocation } from "../shared/utils/portfolio";

const delay = (ms=200) => new Promise(res => setTimeout(res, ms));

export async function getPositions(): Promise<(Position & { weightPct: number })[]> {
  await delay();
  const enriched = enrichPositions(POSITIONS_MOCK);
  const { positions } = calcWeights(enriched);
  return positions as (Position & { weightPct: number })[];
}

export async function getAllocation(by: "sector" | "class" = "sector"): Promise<AllocationSlice[]> {
  await delay();
  const positions = await getPositions();
  return groupAllocation(positions, by);
}
