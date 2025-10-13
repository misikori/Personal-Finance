import { Box, Typography, Stack, Button } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { ROUTES } from "../../core/RoutesConfig";

export default function LandingPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ minHeight: "100vh", display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", bgcolor: "background.default" }}>
      <Typography variant="h3" fontWeight={700} gutterBottom>
        Welcome to Personal Finance !!!
      </Typography>
      <Stack direction="row" spacing={3} sx={{ mt: 4 }}>
        <Button variant="contained" size="large" onClick={() => navigate(ROUTES.PUBLIC.LOGIN,)}>Login</Button>
        <Button variant="outlined" size="large" onClick={() => navigate(ROUTES.PUBLIC.SIGNUP)}>Sign Up</Button>
      </Stack>
    </Box>
  );
}
