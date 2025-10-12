import { Paper, Stack, Typography } from "@mui/material";
import { fmtCurrency } from "../../../../shared/utils/format";
import { PortfolioSummaryProps } from "./PortfolioSummaryProps";


export default function PortfolioSummary({ totalMarketValue, positionsCount }: PortfolioSummaryProps) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
        <Typography variant="body1"><strong>Total Market Value:</strong> {fmtCurrency(totalMarketValue)}</Typography>
        <Typography variant="body1"><strong>Positions:</strong> {positionsCount}</Typography>
      </Stack>
    </Paper>
  );
}
