import { Card, CardContent, Divider, List, ListItem, ListItemText, Stack, Typography } from "@mui/material";
import type { TopMover } from "../../types";

type Props = { movers: TopMover[] };

export default function TopMovers({ movers }: Props) {
  return (
    <Card variant="outlined" sx={{ borderRadius: 3, height: "100%" }}>
      <CardContent>
        <Stack spacing={1}>
          <Typography variant="subtitle1" fontWeight={700}>Top Movers</Typography>
          <Divider />
          <List dense disablePadding>
            {movers.map(m => {
              const sign = m.changePct >= 0 ? "+" : "-";
              const color = m.changePct >= 0 ? "success.main" : "error.main";
              return (
                <ListItem key={m.symbol} disableGutters sx={{ py: 0.5 }}>
                  <ListItemText
                    primary={<Typography fontWeight={600}>{m.symbol}</Typography>}
                    secondary={m.name}
                  />
                  <Typography sx={{ color }}>{sign}{Math.abs(m.changePct).toFixed(2)}%</Typography>
                </ListItem>
              );
            })}
          </List>
        </Stack>
      </CardContent>
    </Card>
  );
}
