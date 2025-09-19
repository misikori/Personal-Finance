import { styled } from "@mui/material/styles";
import { AppBar, Toolbar } from "@mui/material";

export const ShellAppBar = styled(AppBar)(({ theme }) => ({
  borderBottom: `1px solid ${theme.palette.divider}`,
  backdropFilter: "saturate(180%) blur(6px)",
}));

export const ShellToolbar = styled(Toolbar)({});

export const Main = styled("main", {
  shouldForwardProp: (prop) => prop !== "shifted" && prop !== "mini",
})<{ shifted?: boolean; mini?: boolean }>(({ theme }) => ({
  flexGrow: 1,
  minHeight: "100vh",
  transition: theme.transitions.create(["margin", "width"], {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.shortest,
  }),
  padding: theme.spacing(2),
}));