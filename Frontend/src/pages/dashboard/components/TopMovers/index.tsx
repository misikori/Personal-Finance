import { Card, CardContent, Divider, List, ListItem, ListItemText, Stack, Typography, Box } from "@mui/material";
import type { TopMover } from "../../types";

type Props = { movers: TopMover[] };

export default function TopMovers({ movers }: Props) {
  if (!movers?.length) return null;

  return (
    <Card variant="outlined" sx={{ borderRadius: 3, height: "100%" }}>
      <CardContent sx={{ py: 1.25, px: 1.5 }}>
        <Stack spacing={1}>
          <Typography variant="subtitle1" fontWeight={700}>Top Movers</Typography>
          <Divider />
          <List dense disablePadding>
            {movers.map((m) => {
              const sign = m.changePct >= 0 ? "+" : "-";
              const color = m.changePct >= 0 ? "success.main" : "error.main";
              return (
                <ListItem key={m.symbol} disableGutters sx={{ py: 0.5 }}>
                  <ListItemText
                    primary={<Typography fontWeight={600}>{m.symbol}</Typography>}
                    secondary={<Typography variant="caption" color="text.secondary">{m.name}</Typography>}
                  />
                  <Box sx={{ color, fontVariantNumeric: "tabular-nums" }}>
                    {sign}{Math.abs(m.changePct).toFixed(2)}%
                  </Box>
                </ListItem>
              );
            })}
          </List>
        </Stack>
      </CardContent>
    </Card>
  );
}
