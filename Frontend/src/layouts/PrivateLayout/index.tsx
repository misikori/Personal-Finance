import * as React from "react";
import { Box, IconButton, Toolbar, useMediaQuery } from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import { useTheme } from "@mui/material/styles";
import Sidebar from "../../components/sidebar";
import { ShellAppBar, ShellToolbar, Main } from "./styles";

export default function PrivateLayout({ children }: { children: React.ReactNode }) {
  const theme = useTheme();
  const isPermanent = useMediaQuery(theme.breakpoints.up("md"));
  const [open, setOpen] = React.useState(true);
  const toggle = React.useCallback(() => setOpen(o => !o), []);

  React.useEffect(() => {
    setOpen(isPermanent); 
  }, [isPermanent]);
  const contentMarginLeft = isPermanent ? (open ? "22vw" : "7vw") : 0;

  return (
    <Box sx={{ display: "flex", minHeight: "100vh" }}>
      <ShellAppBar position="fixed" elevation={0}>
        <ShellToolbar>
          {(!isPermanent || !open) && (
            <IconButton edge="start" color="inherit" aria-label="open navigation" onClick={toggle}>
              <MenuIcon />
            </IconButton>
          )}
          <Box sx={{ flex: 1 }} />
        </ShellToolbar>
      </ShellAppBar>

      <Sidebar open={open} onToggle={toggle} permanent={isPermanent} />

      <Main sx={{ ml: contentMarginLeft }}>
        <Toolbar />
        {children}
      </Main>
    </Box>
  );
}
