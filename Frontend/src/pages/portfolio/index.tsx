import { Typography, Box } from "@mui/material";
import AllocationChart from "./components/AllocationChart";
import PortfolioSummary from "./components/PortfolioSummary";
import PositionsTable from "./components/PositionsTable";
import { getCurrentUser } from "../../auth/store/authStore";
import { usePortfolioSummary, usePortfolioDistribution } from "../../domain/portfolio/hooks/usePortfolio";


export default function PortfolioPage() {
  const user = getCurrentUser();
  const username = user?.username ?? user?.email ?? "";
  // You can make baseCurrency dynamic if needed
  const baseCurrency = "USD";

  // Fetch summary and allocation using new hooks
  const summaryQ = usePortfolioSummary(username, { baseCurrency });
  const allocationQ = usePortfolioDistribution(username, { baseCurrency });

  // Format positions for table
  const positionsRaw = summaryQ.data?.positions ?? [];
  const totalMV = summaryQ.data?.currentValue ?? 0;
  const positionsWithWeights = positionsRaw.map(p => ({
    ...p,
    weightPct: totalMV ? (p.currentValue / totalMV) * 100 : 0,
  }));
  const allocation = allocationQ.data?.holdings?.map(h => ({
    label: h.symbol,
    value: h.percentage,
    color: h.color,
  })) ?? [];
  const loading = summaryQ.isLoading || allocationQ.isLoading;

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>Portfolio</Typography>
      <Box sx={{ mb: 2 }}>
          <PortfolioSummary totalMarketValue={totalMV} positionsCount={positionsWithWeights.length} />
      </Box>
      <Box sx={{ display: 'flex', gap: 2, flexWrap: { xs: 'wrap', md: 'nowrap' } }}>
        <Box sx={{ flex: 2, minWidth: 300 }}>
          <PositionsTable rows={positionsWithWeights} />
        </Box>
        <Box sx={{ flex: 1, minWidth: 280 }}>
          <AllocationChart data={allocation} title="Allocation by Sector" />
        </Box>
      </Box>
      {loading && <Typography sx={{ mt: 2 }} color="text.secondary">Loading…</Typography>}
    </Box>
  );
}
