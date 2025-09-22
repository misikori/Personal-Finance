import { useMemo } from "react";
import { Box, Paper, Typography, Stack } from "@mui/material";
import { AllocationChartProps } from "./props";
import {
  rootPaperSx, titleSx, containerStackSx, svgBoxSx, legendSx, legendItemDotSx
} from "./styles";
import { useTheme } from "@mui/material/styles";

export default function AllocationChart({
  data,
  title = "Allocation",
  legend = true,
}: AllocationChartProps) {
  const theme = useTheme();

  // pick a cycle of theme-aware colors (readable on dark & light)
  const paletteCycle = useMemo(() => ([
    theme.palette.primary.main,
    theme.palette.success.main,
    theme.palette.info.main,
    theme.palette.warning.main,
    theme.palette.secondary.main,
    theme.palette.error.main,
  ]), [theme.palette]);

  const R = 70, STROKE = 22, C = R + STROKE;
  const total = 100;

  const paths = useMemo(() => {
    let acc = 0;
    return data.map((s, i) => {
      const frac = Math.max(0, s.value) / total;
      const start = acc * 2 * Math.PI;
      const end = (acc + frac) * 2 * Math.PI;
      acc += frac;

      const large = end - start > Math.PI ? 1 : 0;
      const x1 = C + R * Math.cos(start), y1 = C + R * Math.sin(start);
      const x2 = C + R * Math.cos(end),   y2 = C + R * Math.sin(end);
      const d = `M ${x1} ${y1} A ${R} ${R} 0 ${large} 1 ${x2} ${y2}`;

      return {
        d,
        i,
        label: s.label,
        value: s.value,
        color: paletteCycle[i % paletteCycle.length],
      };
    });
  }, [data, paletteCycle]);

  return (
    <Paper variant="outlined" sx={rootPaperSx}>
      <Typography variant="h6" sx={titleSx}>{title}</Typography>

      <Stack direction={{ xs: "column", md: "row" }} spacing={2} sx={containerStackSx}>
        <Box
          component="svg"
          width={C * 2}
          height={C * 2}
          viewBox={`0 0 ${C * 2} ${C * 2}`}
          aria-label="allocation-donut"
          sx={svgBoxSx}
        >
          <circle
            cx={C}
            cy={C}
            r={R}
            fill="none"
            stroke={theme.palette.divider}
            strokeWidth={STROKE}
          />
          {paths.map(p => (
            <path
              key={p.i}
              d={p.d}
              strokeWidth={STROKE}
              strokeLinecap="butt"
              fill="none"
              stroke={p.color}
              opacity={0.9}
            />
          ))}
          {/* donut hole */}
          <circle cx={C} cy={C} r={R - STROKE/2} fill={theme.palette.background.paper} />
        </Box>

        {legend && (
          <Stack spacing={1} sx={legendSx}>
            {data.map((s, i) => (
              <Stack key={i} direction="row" justifyContent="space-between" alignItems="center">
                <Box sx={{ display: "flex", alignItems: "center" }}>
                  <Box sx={{ ...legendItemDotSx, backgroundColor: paletteCycle[i % paletteCycle.length] }} />
                  <Typography variant="body2">{s.label}</Typography>
                </Box>
                <Typography variant="body2" color="text.secondary">{s.value.toFixed(1)}%</Typography>
              </Stack>
            ))}
          </Stack>
        )}
      </Stack>
    </Paper>
  );
}
