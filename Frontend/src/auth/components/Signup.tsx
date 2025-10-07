import { useState } from "react";
import { Alert, Box, Button, Container, Stack, TextField, Typography } from "@mui/material";
import { useAuth } from "../../auth/hooks/useAuth";

export default function SignUp() {
  const { signup, isLoading } = useAuth();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName]   = useState("");
  const [userName, setUserName]   = useState("");
  const [phoneNumber, setPhone]   = useState("");
  const [email, setEmail]         = useState("");
  const [password, setPassword]   = useState("");

  const [ok, setOk] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setOk(false);
    try {
      await signup({ firstName, lastName, userName, password, email, phoneNumber });
      setOk(true);
    } catch (err: any) {
      // try to surface ProblemDetails messages from backend
      const msg = err?.message ?? "Signup failed";
      setError(msg);
    }
  };

  return (
    <Container maxWidth="sm" sx={{ py: 6 }}>
      <Typography variant="h4" gutterBottom>Sign Up</Typography>
      <Box component="form" onSubmit={onSubmit}>
        <Stack spacing={2}>
          {ok && <Alert severity="success">Account created. Check your email or try logging in.</Alert>}
          {error && <Alert severity="error">{error}</Alert>}

          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <TextField label="First name" required fullWidth value={firstName} onChange={e=>setFirstName(e.target.value)} />
            <TextField label="Last name"  required fullWidth value={lastName}  onChange={e=>setLastName(e.target.value)} />
          </Stack>

          <TextField label="Username" required fullWidth value={userName} onChange={e=>setUserName(e.target.value)} />
          <TextField label="Phone number" required fullWidth value={phoneNumber} onChange={e=>setPhone(e.target.value)} />
          <TextField label="Email" type="email" required fullWidth value={email} onChange={e=>setEmail(e.target.value)} />
          <TextField label="Password" type="password" required fullWidth value={password} onChange={e=>setPassword(e.target.value)} />

          <Button variant="contained" type="submit" disabled={isLoading}>Create account</Button>
        </Stack>
      </Box>
    </Container>
  );
}
