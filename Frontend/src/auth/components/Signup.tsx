import { useState } from "react";
import { Alert, Box, Button, Container, Stack, TextField, Typography } from "@mui/material";
import { useAuth } from "../../auth/hooks/useAuth";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../../core/RoutesConfig";

export default function SignUp() {
  const { signup, isLoading } = useAuth();
  const navigate = useNavigate();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName]   = useState("");
  const [userName, setUserName]   = useState("");
  const [phoneNumber, setPhone]   = useState("");
  const [email, setEmail]         = useState("");
  const [password, setPassword]   = useState("");

  const [ok, setOk] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Validation state
  const [validationError, setValidationError] = useState<string | null>(null);

  const validate = () => {
    if (!firstName || !lastName || !userName || !phoneNumber || !email || !password) {
      return "All fields are required.";
    }
    // Simple email regex
    if (!/^\S+@\S+\.\S+$/.test(email)) {
      return "Please enter a valid email address.";
    }
    if (password.length < 8) {
      return "Password must be at least 8 characters.";
    }
    return null;
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setOk(false);
    const vError = validate();
    setValidationError(vError);
    if (vError) return;
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
          {validationError && <Alert severity="warning">{validationError}</Alert>}

          <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
            <TextField label="First name" required fullWidth value={firstName} onChange={e=>setFirstName(e.target.value)} />
            <TextField label="Last name"  required fullWidth value={lastName}  onChange={e=>setLastName(e.target.value)} />
          </Stack>

          <TextField label="Username" required fullWidth value={userName} onChange={e=>setUserName(e.target.value)} />
          <TextField label="Phone number" required fullWidth value={phoneNumber} onChange={e=>setPhone(e.target.value)} />
          <TextField label="Email" type="email" required fullWidth value={email} onChange={e=>setEmail(e.target.value)} />
          <TextField label="Password" type="password" required fullWidth value={password} onChange={e=>setPassword(e.target.value)} />

          <Button variant="contained" type="submit" disabled={isLoading}>Create account</Button>
          <Box textAlign="center" sx={{ mt: 2 }}>
            <Typography variant="body2" sx={{ mb: 1 }}>
              Already have an account?
            </Typography>
            <Button variant="text" onClick={() => navigate(ROUTES.PUBLIC.LOGIN)}>Login</Button>
          </Box>
        </Stack>
      </Box>
    </Container>
  );
}
