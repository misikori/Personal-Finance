import { CircularProgress, Box } from "@mui/material";

const Loader = () => (
  <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
    <CircularProgress />
  </Box>
);

export default Loader;