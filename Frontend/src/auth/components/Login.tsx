import { Button, Container, Stack, Typography } from "@mui/material";
import { useAuth } from "../../auth/hooks/useAuth";

export default function Login() {
  const { login } = useAuth();
  return (
    <Container sx={{ py: 4 }}>
      <Typography variant="h4" gutterBottom>Login</Typography>
      <Stack direction="row" spacing={2}>
        <Button variant="contained" onClick={() => login(["Users"])}>Login as User</Button>
        <Button variant="outlined" onClick={() => login(["Admins"])}>Login as Admin</Button>
      </Stack>
    </Container>
  );
}
