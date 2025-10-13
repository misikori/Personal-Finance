// src/pages/portfolio/index.tsx
import { Typography, Box, Paper, Stack, Alert, Skeleton, Button, Autocomplete, TextField, Divider } from "@mui/material";
import AllocationChart from "./components/AllocationChart";
import PortfolioSummary from "./components/PortfolioSummary";
import PositionsTable from "./components/PositionsTable";
import { getCurrentUser } from "../../auth/store/authStore";
import { useMemo, useState } from "react";
import type { RecommendationsDto, RecommendationsItemDto } from "../../domain/portfolio/types/recomendation";
import { usePortfolioSummary, usePortfolioDistribution, useRecommendations, usePrediction } from "../../domain/portfolio/hooks/usePortfolio";
import { fmtCurrency } from "../../shared/utils/format";

export default function PortfolioPage() {
  const user = getCurrentUser();
  // if your API expects username/email, swap id->username/email accordingly
  const username = user?.id ?? user?.email ?? "";
  const baseCurrency = "USD";

  const summaryQ = usePortfolioSummary(username, { baseCurrency });
  const allocationQ = usePortfolioDistribution(username, { baseCurrency });

  // --- derive positions + totals
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
      label: h.symbol,
      value: h.percentage,
      color: h.color,
    })) ?? []
  ), [allocationQ.data?.holdings]);

  // --- controls: symbol select + manual fetch buttons
  const symbols = useMemo(() => (summaryQ.data?.positions ?? []).map(p => p.symbol), [summaryQ.data?.positions]);

  const [symbol, setSymbol] = useState<string>( "");

  // Recommendations: lazy (manual)
  const recsQ = useRecommendations({ symbols, enabled: false });
  const doFetchRecs = () => recsQ.refetch();

  // Prediction: lazy (manual)
  const predictionQ = usePrediction(symbol);
  const doPredict = () => {
    if (symbol) predictionQ.refetch();
  };

  // Loading/error
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
      <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 1.5, gap: 1 }}>
        <Typography variant="h5" fontWeight={700}>Portfolio</Typography>

        {/* Controls: compact */}
        <Stack direction="row" spacing={1} alignItems="center" sx={{ flexWrap: "wrap" }}>
          <Autocomplete
            size="small"
            sx={{ minWidth: 200 }}
            options={symbols}
            value={symbol}
            onChange={(_e, v) => setSymbol(v ?? "")}
            renderInput={(params) => <TextField {...params} label="Symbol" />}
            disabled={!symbols.length}
          />
          <Button
            size="small"
            variant="outlined"
            onClick={doPredict}
            disabled={!symbol || predictionQ.isFetching}
          >
            {predictionQ.isFetching ? "Predicting…" : "Predict"}
          </Button>
          <Divider flexItem orientation="vertical" sx={{ mx: 1 }} />
          <Button
            size="small"
            variant="contained"
            onClick={doFetchRecs}
            disabled={recsQ.isFetching || symbols.length === 0}
          >
            {recsQ.isFetching ? "Loading…" : "Get Recommendations"}
          </Button>
        </Stack>
      </Stack>

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
          <Paper variant="outlined" sx={{ mt: 1.5, p: 1.5 }}>
            <Stack direction={{ xs: "column", sm: "row" }} spacing={1.25}>
              <Typography variant="body2" color={totalGainLoss >= 0 ? "success.main" : "error.main"}>
                <strong>Total Gain/Loss:</strong>{" "}
                {fmtCurrency(totalGainLoss)} ({gainLossPercentage.toFixed(2)}%)
              </Typography>
            </Stack>
          </Paper>
        )}
      </Box>

      {/* Main area */}
      <Stack sx={{ display: 'flex', gap: 1.5, flexWrap: { xs: 'wrap', md: 'nowrap' } }}>
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

          {/* Prediction result card */}
          {(predictionQ.isFetching || predictionQ.data || predictionQ.error) && (
            <Paper variant="outlined" sx={{ mt: 1.5, p: 1.25 }}>
              <Typography variant="subtitle2" sx={{ mb: 0.5 }}>
                Prediction {symbol ? `(${symbol})` : ""}
              </Typography>
              {predictionQ.isFetching && <Skeleton variant="text" width={160} />}
              {predictionQ.error && <Alert severity="error">Failed to fetch prediction.</Alert>}
              {predictionQ.data && (
                <Stack spacing={0.5}>
                  <Typography variant="body2">Current: {fmtCurrency(predictionQ.data.currentPrice)}</Typography>
                  <Typography variant="body2">Predicted: {fmtCurrency(predictionQ.data.predictedPrice)}</Typography>
                  <Typography variant="body2">
                    Δ%: {predictionQ.data.predictedChangePercent.toFixed(2)}% | Conf: {(predictionQ.data.confidence <= 1 ? predictionQ.data.confidence * 100 : predictionQ.data.confidence).toFixed(0)}%
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {predictionQ.data.method} • {new Date(predictionQ.data.generatedAt).toLocaleString()}
                  </Typography>
                </Stack>
              )}
            </Paper>
          )}
        </Box>
      </Stack>

      {/* Recommendations (shown after button) */}
      {recsQ.data && (
        <Paper variant="outlined" sx={{ mt: 2, p: 1.5 }}>
          <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
            Recommendations{" "}
            <Typography component="span" variant="body2" color="text.secondary">
              ({recsQ.data.timeframe})
            </Typography>
          </Typography>
          <Stack spacing={0.75}>
            {(["buyRecommendations","sellRecommendations","holdRecommendations"] as const).map(type => {
              const items = (recsQ.data as RecommendationsDto)[type] as RecommendationsItemDto[];
              if (!items?.length) return null;
              const label = type.replace("Recommendations","");

              return (
                <Box key={type}>
                  <Typography variant="subtitle2" sx={{ mb: 0.25 }}>{label}:</Typography>
                  {items.map((rec, idx) => (
                    <Typography key={rec.symbol + idx} variant="body2">
                      {rec.symbol}: {rec.action} ({rec.strength})
                      {" – "}exp {rec.expectedChangePercent?.toFixed?.(2) ?? rec.expectedChangePercent}%,
                      conf {typeof rec.confidence === "number"
                        ? (rec.confidence <= 1 ? (rec.confidence*100).toFixed(0) : rec.confidence.toFixed(0))
                        : rec.confidence}%
                      {" – "}{rec.reason}
                    </Typography>
                  ))}
                </Box>
              );
            })}
          </Stack>
        </Paper>
      )}

      {loading && <Typography sx={{ mt: 1.5 }} color="text.secondary">Loading…</Typography>}
      {error && <Typography sx={{ mt: 1.5 }} color="error.main">Some sections failed to load.</Typography>}
    </Box>
  );
}
