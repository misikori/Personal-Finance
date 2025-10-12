import * as React from "react";
import { Box, Grid, Stack, Typography } from "@mui/material";
import KpiCard from "./components/KpiCard";
import RecentActivity from "./components/RecentActivity";
// import TopMovers from "./components/TopMovers";
import { WalletsPanel } from "./components/WalletsPanel";
import { CurrencyRatesPanel } from "./components/CurrencyRatesPanel";
import { CurrencyConvertPanel } from "./components/CurrencyConvertPanel";
import { portfolioData } from "../../domain/portfolio/services/PortfolioDataService";
// import { marketService } from "../../domain/portfolio/services/MarketDataService";
import { Kpi, RecentTransaction } from "./types";
import { getCurrentUser } from "../../auth/store/authStore";

export default function DashboardPage() {
  const [kpis, setKpis] = React.useState<Kpi[]>([]);
  const [recent, setRecent] = React.useState<RecentTransaction[]>([]);
  // Removed unused movers and loading state

  React.useEffect(() => {
    let mounted = true;
    (async () => {
      try {
        // Replace with real service calls
        // Example: get current user from wallet panel logic
        const username = getCurrentUser()?.id ?? "demo";
        // Portfolio summary for KPIs
        const summary = await portfolioData.summary("niko");
        setKpis([
          { id: "totalBalance", label: "Total Balance", value: summary.currentValue.toLocaleString(), trend: summary.gainLossPercentage > 0 ? "up" : "down", sublabel: `GL: ${summary.gainLossPercentage.toFixed(2)}%` },
          { id: "dailyPL", label: "Daily P/L", value: "-", trend: "flat" },
          { id: "mtdReturn", label: "MTD Return %", value: "-", trend: "flat" },
          { id: "cash", label: "Cash", value: summary.totalInvested.toLocaleString() },
        ]);
        // Transactions
        const txs = await portfolioData.transactions("niko");
        setRecent(txs.slice(0, 5).map(tx => ({
          id: tx.id,
          ts: tx.transactionDate,
          symbol: tx.symbol,
          side: tx.type === "Buy" ? "BUY" : "SELL",
          qty: tx.quantity,
          price: tx.pricePerShare,
          amount: tx.totalValue,
        })));
        // Top movers (fallback to mock if not available)
        // If marketService.price or similar is available, fetch real movers
  // Removed TopMovers (no real data)
      } catch (e) {
        setKpis([]);
        setRecent([]);
  // Removed TopMovers (no real data)
      } finally {
  // Removed loading state
      }
    })();
    return () => { mounted = false; };
  }, []);

  return (
    <Stack spacing={3} sx={{ maxWidth: "100%" }}>
      <Typography variant="h5" fontWeight={800}>Dashboard</Typography>
      {/* KPI Cards Row - Flexbox */}
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        {kpis.map(k => (
          <Box key={k.id} sx={{ flex: '1 1 220px', minWidth: 220 }}>
            <KpiCard label={k.label} value={k.value} sublabel={k.sublabel} trend={k.trend} />
          </Box>
        ))}
      </Box>
      {/* Main area - Flexbox */}
      <Box sx={{ display: 'flex', gap: 2, flexWrap: { xs: 'wrap', md: 'nowrap' } }}>
        <Box sx={{ flex: 2, minWidth: 300 }}>
          <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 1 }}>Recent Activity</Typography>
          <RecentActivity rows={recent} />
        </Box>
        <Box sx={{ flex: 1, minWidth: 280 }}>
          <Stack spacing={2}>
            <WalletsPanel />
            <CurrencyRatesPanel />
            <CurrencyConvertPanel />
          </Stack>
        </Box>
      </Box>
    </Stack>
  );
}
