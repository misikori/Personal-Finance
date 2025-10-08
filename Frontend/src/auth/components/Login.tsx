import { useState } from "react";
import { Alert, Box, Button, Container, Stack, TextField, Typography } from "@mui/material";
import { useAuth } from "../../auth/hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../../core/RoutesConfig";

export default function Login() {
  const { login, isLoading } = useAuth();
  const [userName, setUserName] = useState("demo@demo.test");
  const [password, setPassword] = useState("demo123!");
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
       await login(userName, password);
      navigate(ROUTES.PRIVATE.DASHBOARD, { replace: true });
    } catch (err: any) {
      setError(err?.message ?? "Login failed");
    }
  };

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
      <Typography variant="h4" gutterBottom>Login</Typography>
      <Box component="form" onSubmit={onSubmit}>
        <Stack spacing={2}>
          {error && <Alert severity="error">{error}</Alert>}
          <TextField
            label="Username"
            type="text"                                  
            required fullWidth
            value={userName}
            onChange={e => setUserName(e.target.value)} 
          />
          <TextField
            label="Password" type="password" required fullWidth
            value={password} onChange={e => setPassword(e.target.value)}
          />
          <Button variant="contained" type="submit" disabled={isLoading}>Sign in</Button>
        </Stack>
      </Box>
    </Container>
  );
}
