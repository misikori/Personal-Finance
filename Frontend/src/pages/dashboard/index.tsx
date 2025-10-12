import * as React from "react";
import { Box, Stack, Typography, Skeleton } from "@mui/material";
import KpiCard from "./components/KpiCard";
import RecentActivity from "./components/RecentActivity";
// import TopMovers from "./components/TopMovers";
import { WalletsPanel } from "./components/WalletsPanel";
import { CurrencyRatesPanel } from "./components/CurrencyRatesPanel";
import { CurrencyConvertPanel } from "./components/CurrencyConvertPanel";
import { portfolioData } from "../../domain/portfolio/services/PortfolioDataService";
import { Kpi, RecentTransaction } from "./types";
import { getCurrentUser } from "../../auth/store/authStore";

export default function DashboardPage() {
  const [kpis, setKpis] = React.useState<Kpi[]>([]);
  const [recent, setRecent] = React.useState<RecentTransaction[]>([]);
  const [loading, setLoading] = React.useState(true);

  React.useEffect(() => {
    (async () => {
      try {
        const username = getCurrentUser()?.id ?? getCurrentUser()?.email ?? "demo";
        const summary = await portfolioData.summary(username);
        const txs = await portfolioData.transactions(username);

        setKpis([
          {
            id: "totalBalance",
            label: "Total Balance",
            value: summary.currentValue.toLocaleString(undefined, { maximumFractionDigits: 2 }),
            trend: summary.gainLossPercentage > 0 ? "up" : summary.gainLossPercentage < 0 ? "down" : "flat",
            sublabel: `GL: ${summary.gainLossPercentage.toFixed(2)}%`,
          },
          {
            id: "dailyPL",
            label: "Daily P/L",
            value: "-", trend: "flat",
            sublabel: undefined,
          },
          {
            id: "mtdReturn",
            label: "MTD Return %",
            value: "-", trend: "flat",
            sublabel: undefined,
          },
          {
            id: "cash",
            label: "Total Invested",
            value: summary.totalInvested.toLocaleString(undefined, { maximumFractionDigits: 2 }),
            trend: "flat",
          },
        ]);

        setRecent(
          (txs ?? []).slice(0, 5).map(tx => ({
            id: tx.id,
            ts: tx.transactionDate,
            symbol: tx.symbol,
            side: (tx.type?.toUpperCase?.() === "BUY") ? "BUY" : "SELL",
            qty: tx.quantity,
            price: tx.pricePerShare,
            amount: tx.totalValue,
          }))
        );
      } catch {
        setKpis([]);
        setRecent([]);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  return (
    <Stack spacing={2.25} sx={{ maxWidth: "100%" }}>
      <Typography variant="h6" fontWeight={800}>Dashboard</Typography>

      {/* KPI Cards Row */}
      <Box sx={{ display: "flex", gap: 1.5, flexWrap: "wrap" }}>
        {loading && Array.from({ length: 4 }).map((_, i) => (
          <Box key={i} sx={{ flex: "1 1 220px", minWidth: 220 }}>
            <Skeleton variant="rounded" height={92} />
          </Box>
        ))}
        {!loading && kpis.map(k => (
          <Box key={k.id} sx={{ flex: "1 1 220px", minWidth: 220 }}>
            <KpiCard label={k.label} value={k.value} sublabel={k.sublabel} trend={k.trend} />
          </Box>
        ))}
      </Box>

      {/* Main area */}
      <Box sx={{ display: "flex", gap: 1.5, flexWrap: { xs: "wrap", md: "nowrap" } }}>
        <Box sx={{ flex: 2, minWidth: 300 }}>
          <Stack spacing={1.25}>
            <Typography variant="subtitle2" fontWeight={700}>Recent Activity</Typography>
            {loading ? (
              <Skeleton variant="rounded" height={240} />
            ) : (
              <RecentActivity rows={recent} />
            )}
            <Box sx={{ display: "flex", gap: 1.5, flexWrap: { xs: "wrap", md: "nowrap" } }}>
              <CurrencyRatesPanel />
              <CurrencyConvertPanel />
            </Box>
          </Stack>
        </Box>

        <Box sx={{ flex: 1, minWidth: 280 }}>
          <Stack spacing={1.25}>
            <WalletsPanel />
            {/* <TopMovers movers={...} /> */}
          </Stack>
        </Box>
      </Box>
    </Stack>
  );
}
