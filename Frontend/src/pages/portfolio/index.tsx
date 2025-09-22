import { Grid, Typography } from "@mui/material";
import AllocationChart from "./components/AllocationChart";
import PortfolioSummary from "./components/PortfolioSummary";
import PositionsTable from "./components/PositionsTable";
import { usePortfolio } from "./hooks/usePortfolio";


export default function PortfolioPage() {
  const { positions, allocation, totalMV, loading } = usePortfolio();

  return (
    <div>
      <Typography variant="h4" sx={{ mb: 2 }}>Portfolio</Typography>

      {/* Summary on top */}
      <Grid container spacing={2} columns={{ xs: 1, md: 12 }} sx={{ mb: 1 }}>
        <Grid size={{ xs: 1, md: 12 }}>
          <PortfolioSummary totalMarketValue={totalMV} positionsCount={positions.length} />
        </Grid>
      </Grid>

      {/* Main content: table + chart */}
      <Grid container spacing={2} columns={{ xs: 1, md: 6 }}>
        <Grid size={{ xs: 1, md: 8 }}>
          <PositionsTable rows={positions} />
        </Grid>
      </Grid>
        <Grid size={{ xs: 1, md: 4 }}>
          <Grid>
          <AllocationChart data={allocation} title="Allocation by Sector" />
          </Grid>
        </Grid>
      {loading && <Typography sx={{ mt: 2 }} color="text.secondary">Loading…</Typography>}
    </div>
  );
}
