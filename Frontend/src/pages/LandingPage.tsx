import { Container, Typography } from "@mui/material";

export default function LandingPage() {
  return (
    <Container sx={{ py: 4 ,maxWidth:'100%'}}>
      <Typography variant="h4">Landing</Typography>
      <Typography>Public page.</Typography>
    </Container>
  );
}