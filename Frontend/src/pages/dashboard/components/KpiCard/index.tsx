import { Card, CardContent, Stack, Typography, Box } from "@mui/material";
import ArrowDropUpIcon from "@mui/icons-material/ArrowDropUp";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import RemoveIcon from "@mui/icons-material/Remove";
import { KpiCardProps } from "./props";

export default function KpiCard({ label, value, sublabel, trend }: KpiCardProps) {
  const TrendIcon = trend === "up" ? ArrowDropUpIcon : trend === "down" ? ArrowDropDownIcon : RemoveIcon;

  return (
    <Card variant="outlined" sx={{ borderRadius: 3, height: "100%" }}>
      <CardContent>
        <Stack spacing={1}>
          <Typography variant="body2" color="text.secondary">{label}</Typography>
          <Typography variant="h5" fontWeight={700}>{value}</Typography>
          {sublabel && (
            <Box display="flex" alignItems="center" gap={0.5}>
              <TrendIcon fontSize="small" />
              <Typography variant="caption" color="text.secondary">{sublabel}</Typography>
            </Box>
          )}
        </Stack>
      </CardContent>
    </Card>
  );
}