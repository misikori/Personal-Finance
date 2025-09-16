import ConfigInfo from "../components/ConfigInfo";
import { useEffect, useState } from "react";
import { userService } from "../user/services/userService";
import { Button, Container, Stack, Typography } from "@mui/material";
import { useAuth } from "../auth/hooks/useAuth";

export default function Dashboard() {
  const [me, setMe] = useState<any>(null);
  const [err, setErr] = useState<string | null>(null);
  const { login, logout, isAuthenticated } = useAuth();

  const load = () => {
    setErr(null);
    userService.getMe()
      .then(setMe)
      .catch((e) => setErr(e?.message ?? "error"));
  };

  useEffect(() => {
    load();
  }, []);

  return (
    <Container sx={{ py: 4 }}>
      <Typography variant="h4">Dashboard</Typography>
      <ConfigInfo/>
      <Stack direction="row" spacing={2} sx={{ my: 2 }}>
        {!isAuthenticated ? (
          <>
            <Button variant="contained" onClick={() => login(["Users"]).then(load)}>Login as User</Button>
            <Button variant="outlined" onClick={() => login(["Admins"]).then(load)}>Login as Admin</Button>
          </>
          
        ) : (
          <Button variant="outlined" color="error" onClick={() => logout().then(() => setMe(null))}>Logout</Button>
        )}
        <Button onClick={load}>Refresh /me</Button>
      </Stack>
      {err && <Typography color="error">Error: {err}</Typography>}
      <pre>{JSON.stringify(me, null, 2)}</pre>
    </Container>
  );
}