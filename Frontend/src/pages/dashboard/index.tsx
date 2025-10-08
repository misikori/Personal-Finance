import * as React from "react";
import { Grid as Grid, Stack, Typography } from "@mui/material";

import KpiCard from "./components/KpiCard";
import RecentActivity from "./components/RecentActivity";
import TopMovers from "./components/TopMovers";
import { getKpisMock, getRecentTransactionsMock, getTopMoversMock } from "./dashboardMocks";
import { Kpi, RecentTransaction, TopMover } from "./types";

export default function DashboardPage() {
  const [kpis, setKpis] = React.useState<Kpi[]>([]);
  const [recent, setRecent] = React.useState<RecentTransaction[]>([]);
  const [movers, setMovers] = React.useState<TopMover[] | null>(null);
  React.useEffect(() => {
    let mounted = true;
    (async () => {
      const [k, r] = await Promise.all([getKpisMock(), getRecentTransactionsMock()]);
      if (!mounted) return;
      setKpis(k);
      setRecent(r.slice(0, 5));

      getTopMoversMock().then(m => mounted && setMovers(m.slice(0, 5)));
    })();
    return () => { mounted = false; };
  }, []);

  return (
    <Stack spacing={3} sx={{maxWidth:'100%'}}>
      <Typography variant="h5" fontWeight={800}>Dashboard</Typography>

      <Grid container spacing={2}>
        {kpis.map(k => (
          <Grid key={k.id} size={{ xs: 12, sm: 6, md: 3 }}>
            <KpiCard label={k.label} value={k.value} sublabel={k.sublabel} trend={k.trend} />
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Typography variant="subtitle1" fontWeight={700} sx={{ mb: 1 }}>Recent Activity</Typography>
          <RecentActivity rows={recent} />
        </Grid>
        <Grid size={{ xs: 12, md: 4 }}>
          {movers && <TopMovers movers={movers} />}
        </Grid>
      </Grid>
    </Stack>
  );
}
