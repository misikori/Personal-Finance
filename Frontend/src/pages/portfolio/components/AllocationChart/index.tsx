import { useMemo } from "react";
import { Box, Paper, Typography, Stack, Tooltip } from "@mui/material";
import { AllocationChartProps } from "./props";
import { useTheme } from "@mui/material/styles";

export default function AllocationChart({
  data,
  title = "Allocation",
  legend = true,
}: AllocationChartProps) {
  const theme = useTheme();

  // ————— Compact spacing tokens —————
  const PAD_Y = 1.25;           // Paper vertical padding
  const PAD_X = 1.5;            // Paper horizontal padding
  const SECTION_GAP = 1;        // Gap between chart and legend
  const LEGEND_ROW_GAP = 0.5;   // Gap inside legend rows
  const TITLE_MB = 0.75;        // Space under title

  // palette
  const paletteCycle = useMemo(
    () => [
      theme.palette.primary.main,
      theme.palette.success.main,
      theme.palette.info.main,
      theme.palette.warning.main,
      theme.palette.secondary.main,
      theme.palette.error.main,
    ],
    [theme.palette]
  );

  // geometry (slightly smaller donut for tighter layout)
  const R = 64, STROKE = 20, C = R + STROKE;
  const SIZE = (R + STROKE) * 2;

// totals
const safeValues = data.map(d => Math.max(0, Number.isFinite(d.value) ? d.value : 0));
const total = safeValues.reduce((s, v) => s + v, 0) || 1;

// detect "single full slice" scenario (e.g., only 1 holding)
const singleIndex = safeValues.findIndex(v => v > 0);
const isSingleFull =
  singleIndex >= 0 &&
  safeValues.filter(v => v > 0).length === 1 &&
  Math.abs(safeValues[singleIndex] / total - 1) < 1e-6;

// build arcs (non-full)
const paths = useMemo(() => {
  if (isSingleFull) return []; // we’ll render a full circle instead
  let acc = 0;
  const EPS = 1e-10;

  return data.map((s, i) => {
    const raw = Math.max(0, Number.isFinite(s.value) ? s.value : 0);
    const frac = raw / total;
    if (frac <= 0) return null;

    const start = acc * 2 * Math.PI;
    // don’t subtract EPS if slice is (almost) 100% — handled via isSingleFull
    const end = (acc + frac - EPS) * 2 * Math.PI;
    acc += frac;

    const large = end - start > Math.PI ? 1 : 0;
    const x1 = C + R * Math.cos(start), y1 = C + R * Math.sin(start);
    const x2 = C + R * Math.cos(end),   y2 = C + R * Math.sin(end);
    const d = `M ${x1} ${y1} A ${R} ${R} 0 ${large} 1 ${x2} ${y2}`;

    return {
      d,
      i,
      label: s.label,
      value: raw,
      color: s.color ?? paletteCycle[i % paletteCycle.length],
    };
  }).filter(Boolean) as Array<{ d: string; i: number; label: string; value: number; color: string }>;
}, [data, paletteCycle, total, isSingleFull]);

  return (
    <Paper
      variant="outlined"
      sx={{
        overflow: "hidden",
        px: PAD_X,
        py: PAD_Y,
      }}
    >
      <Typography variant="subtitle1" sx={{ mb: TITLE_MB, fontWeight: 600 }}>
        {title}
      </Typography>

      <Stack
        direction={{ xs: "column", md: "row" }}
        spacing={SECTION_GAP}
        sx={{ alignItems: "stretch", flexWrap: "wrap" }}
      >
      <Box
        component="svg"
        viewBox={`0 0 ${SIZE} ${SIZE}`}
        aria-label="allocation-donut"
        sx={{ width: "100%", maxWidth: SIZE, height: "auto", display: "block" }}
      >
        {/* base ring */}
        <circle cx={C} cy={C} r={R} fill="none" stroke={theme.palette.divider} strokeWidth={STROKE} />

        {/* FULL SLICE fallback */}
        {isSingleFull && singleIndex >= 0 && (
          <circle
            cx={C}
            cy={C}
            r={R}
            fill="none"
            stroke={(data[singleIndex].color ?? paletteCycle[singleIndex % paletteCycle.length])}
            strokeWidth={STROKE}
            opacity={0.95}
          >
            <title>{`${data[singleIndex].label}: 100.00%`}</title>
          </circle>
        )}

        {/* regular slices */}
        {!isSingleFull && paths.map(p => (
          <path
            key={p.i}
            d={p.d}
            strokeWidth={STROKE}
            strokeLinecap="butt"
            strokeLinejoin="round"
            fill="none"
            stroke={p.color}
            opacity={0.95}
          >
            <title>{`${p.label}: ${((p.value / total) * 100).toFixed(2)}%`}</title>
          </path>
        ))}

        {/* donut hole */}
        <circle cx={C} cy={C} r={R - STROKE / 2} fill={theme.palette.background.paper} />
      </Box>

        {/* Legend: compact rows, no overflow */}
        {legend && (
          <Stack
            spacing={LEGEND_ROW_GAP}
            sx={{
              flex: "1 1 240px",
              minWidth: 200,
              maxWidth: "100%",
              overflow: "hidden",
            }}
          >
            {data.map((s, i) => {
              const val = Math.max(0, Number.isFinite(s.value) ? s.value : 0);
              const pct = (val / total) * 100;
              const color = s.color ?? paletteCycle[i % paletteCycle.length];
              return (
                <Stack
                  key={i}
                  direction="row"
                  alignItems="center"
                  sx={{ gap: 0.75, minWidth: 0 }}
                >
                  <Box
                    sx={{
                      width: 10,
                      height: 10,
                      borderRadius: "50%",
                      backgroundColor: color,
                      flexShrink: 0,
                    }}
                  />
                  <Tooltip title={s.label}>
                    <Typography
                      variant="body2"
                      noWrap
                      sx={{ flex: "1 1 auto", minWidth: 0, lineHeight: 1.3 }}
                    >
                      {s.label}
                    </Typography>
                  </Tooltip>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ flexShrink: 0, lineHeight: 1.3 }}
                  >
                    {Number.isFinite(pct) ? pct.toFixed(1) : "0.0"}%
                  </Typography>
                </Stack>
              );
            })}
          </Stack>
        )}
      </Stack>
    </Paper>
  );
}
