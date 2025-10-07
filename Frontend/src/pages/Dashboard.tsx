import ConfigInfo from "../components/ConfigInfo";
import { useEffect, useState } from "react";
import { userService } from "../user/services/userService";
import { Button, Container, Stack, Typography } from "@mui/material";
import { useAuth } from "../auth/hooks/useAuth";

const DEMO = {
  Users:  { userName: "demo@demo.test",  password: "demo123!"  },
  Admins: { userName: "admin@demo.test", password: "admin123!" },
};

export default function Dashboard() {
  const [me, setMe] = useState<any>(null);
  const [err, setErr] = useState<string | null>(null);
  const { login, logout, isAuthenticated } = useAuth();

  const load = () => {
    setErr(null);
    userService.getMe().then(setMe).catch(e => setErr(e?.message ?? "error"));
  };

  useEffect(() => { load(); }, []);

  const loginAs = async (role: keyof typeof DEMO) => {
    const { userName, password } = DEMO[role];
    await login(userName, password);
    load();
  };

  return (
    <Container sx={{ py: 4 }}>
      <Typography variant="h4">Dashboard</Typography>
      <ConfigInfo/>
      <Stack direction="row" spacing={2} sx={{ my: 2 }}>
        {!isAuthenticated ? (
          <>
            <Button variant="contained" onClick={() => loginAs("Users")}>Login as User</Button>
            <Button variant="outlined"  onClick={() => loginAs("Admins")}>Login as Admin</Button>
          </>
        ) : (
          <Button variant="outlined" color="error" onClick={() => logout().then(() => setMe(null))}>
            Logout
          </Button>
        )}
        <Button onClick={load}>Refresh /me</Button>
      </Stack>
      {err && <Typography color="error">Error: {err}</Typography>}
      <pre>{JSON.stringify(me, null, 2)}</pre>
    </Container>
  );
}
