import { Typography, Box, Paper, Stack, Alert, Skeleton } from "@mui/material";
import AllocationChart from "./components/AllocationChart";
import PortfolioSummary from "./components/PortfolioSummary";
import PositionsTable from "./components/PositionsTable";
import { getCurrentUser } from "../../auth/store/authStore";
import {  useMemo } from "react";
import type { RecommendationsDto, RecommendationsItemDto } from "../../domain/portfolio/types/recomendation";
import { usePortfolioSummary, usePortfolioDistribution, useRecommendations } from "../../domain/portfolio/hooks/usePortfolio";
import { fmtCurrency } from "../../shared/utils/format";

export default function PortfolioPage() {
  const user = getCurrentUser();
  const username = user?.id ?? user?.email ?? "";
  const baseCurrency = "USD";

  const summaryQ = usePortfolioSummary(username, { baseCurrency });
  const allocationQ = usePortfolioDistribution(username, { baseCurrency });
  const recsQ = useRecommendations();

  // local derived state
  const totalMV = summaryQ.data?.currentValue ?? 0;
  const totalGainLoss = summaryQ.data?.totalGainLoss ?? 0;
  const gainLossPercentage = summaryQ.data?.gainLossPercentage ?? 0;

  const positionsWithWeights = useMemo(() => {
    const rows = summaryQ.data?.positions ?? [];
    return rows.map(p => ({
      ...p,
      weightPct: totalMV > 0 ? (p.currentValue / totalMV) * 100 : 0,
    }));
  }, [summaryQ.data?.positions, totalMV]);

  const allocation = useMemo(() => (
    allocationQ.data?.holdings?.map(h => ({
      label: h.symbol,          // or sector if you add it later
      value: h.percentage,      // 0..100
      color: h.color,
    })) ?? []
  ), [allocationQ.data?.holdings]);

  // Loading and error handling
  const loading = summaryQ.isLoading || allocationQ.isLoading;
  const error = summaryQ.error || allocationQ.error;

  if (!username) {
    return (
      <Box>
        <Typography variant="h5" sx={{ mb: 2 }}>Portfolio</Typography>
        <Alert severity="info">Sign in to view your portfolio.</Alert>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" sx={{ mb: 2 }}>Portfolio</Typography>

      {/* Top summary */}
      <Box sx={{ mb: 2 }}>
        {summaryQ.isLoading ? (
          <Skeleton variant="rounded" height={64} />
        ) : summaryQ.error ? (
          <Alert severity="error">Failed to load summary.</Alert>
        ) : (
          <PortfolioSummary
            totalMarketValue={totalMV}
            positionsCount={positionsWithWeights.length}
          />
        )}

        {!summaryQ.isLoading && !summaryQ.error && (
          <Paper variant="outlined" sx={{ mt: 2, p: 2 }}>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
              <Typography variant="body2" color={totalGainLoss >= 0 ? "success.main" : "error.main"}>
                <strong>Total Gain/Loss:</strong>{" "}
                {fmtCurrency(totalGainLoss)} ({gainLossPercentage.toFixed(2)}%)
              </Typography>
            </Stack>
          </Paper>
        )}
      </Box>

      {/* Main area */}
      <Stack sx={{ display: 'flex', gap: 2, flexWrap: { xs: 'wrap', md: 'nowrap' } }}>
        <Box sx={{ flex: 2, minWidth: 300 }}>
          {summaryQ.isLoading ? (
            <Skeleton variant="rounded" height={420} />
          ) : summaryQ.error ? (
            <Alert severity="error">Failed to load positions.</Alert>
          ) : (
            <PositionsTable rows={positionsWithWeights} />
          )}
        </Box>
        <Box sx={{ flex: 1, minWidth: 280 }}>
          {allocationQ.isLoading ? (
            <Skeleton variant="rounded" height={420} />
          ) : allocationQ.error ? (
            <Alert severity="error">Failed to load allocation.</Alert>
          ) : (
            <AllocationChart data={allocation} title="Allocation by Symbol" />
          )}
        </Box>

      </Stack>

      {/* Recommendations */}
      {recsQ.data && (
        <Paper variant="outlined" sx={{ mt: 3, p: 2 }}>
          <Typography variant="h6" sx={{ mb: 1 }}>
            Recommendations <Typography component="span" variant="body2" color="text.secondary">({recsQ.data.timeframe})</Typography>
          </Typography>
          <Stack spacing={1}>
            {(["buyRecommendations","sellRecommendations","holdRecommendations"] as const).map(type => {
              const items = (recsQ.data as RecommendationsDto)[type] as RecommendationsItemDto[];
              if (!items?.length) return null;
              const label = type.replace("Recommendations","");

              return (
                <Box key={type}>
                  <Typography variant="subtitle2" sx={{ mb: 0.5 }}>{label}:</Typography>
                  {items.map((rec, idx) => (
                    <Typography key={rec.symbol + idx} variant="body2">
                      {rec.symbol}: {rec.action} ({rec.strength})
                      {" – "}exp {rec.expectedChangePercent?.toFixed?.(2) ?? rec.expectedChangePercent}%,
                      conf {typeof rec.confidence === "number" ? (rec.confidence <= 1 ? (rec.confidence*100).toFixed(0) : rec.confidence.toFixed(0)) : rec.confidence}%
                      {" – "}{rec.reason}
                    </Typography>
                  ))}
                </Box>
              );
            })}
          </Stack>
        </Paper>
      )}

      {loading && <Typography sx={{ mt: 2 }} color="text.secondary">Loading…</Typography>}
      {error && <Typography sx={{ mt: 2 }} color="error.main">Some sections failed to load.</Typography>}
    </Box>
  );
}
