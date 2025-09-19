import { CssBaseline } from "@mui/material";
import { Box, Container } from "@mui/system";
import { PropsWithChildren } from "react";
import { Outlet } from "react-router-dom";
export default function PublicLayout({ children }: PropsWithChildren) {
  return (
    <Box sx={{ minHeight: "100vh" }}>
      <CssBaseline />
      <Container maxWidth="lg" sx={{ py: 4 }}>
        {children ?? <Outlet />}
      </Container>
    </Box>
  );
}