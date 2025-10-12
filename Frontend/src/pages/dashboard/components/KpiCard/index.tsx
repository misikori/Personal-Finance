import { Card, CardContent, Stack, Typography, Box } from "@mui/material";
import ArrowDropUpIcon from "@mui/icons-material/ArrowDropUp";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import RemoveIcon from "@mui/icons-material/Remove";
import { KpiCardProps } from "./props";

export default function KpiCard({ label, value, sublabel, trend }: KpiCardProps) {
  const TrendIcon =
    trend === "up" ? ArrowDropUpIcon : trend === "down" ? ArrowDropDownIcon : RemoveIcon;

  const trendColor =
    trend === "up" ? "success.main" : trend === "down" ? "error.main" : "text.disabled";

  return (
    <Card variant="outlined" sx={{ borderRadius: 3, height: "100%" }}>
      <CardContent sx={{ py: 1.25, px: 1.5 }}>
        <Stack spacing={0.5}>
          <Typography variant="caption" color="text.secondary">{label}</Typography>
          <Typography variant="h6" fontWeight={700} sx={{ lineHeight: 1.2 }}>
            {value}
          </Typography>

          {sublabel && (
            <Box display="flex" alignItems="center" gap={0.5}>
              <TrendIcon fontSize="small" sx={{ color: trendColor }} />
              <Typography variant="caption" color="text.secondary">{sublabel}</Typography>
            </Box>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}
