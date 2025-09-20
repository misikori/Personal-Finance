import { styled } from "@mui/material/styles";

export const Main = styled("main")(({ theme }) => ({
  flexGrow: 1,
  minHeight: "100vh",
  minWidth:"700wh",
  transition: theme.transitions.create(["margin", "width"], {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.shortest,
  }),
  padding: theme.spacing(2),
}));