import * as React from "react";
import { Box, Toolbar, useMediaQuery } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import { Outlet } from "react-router-dom";         
import Sidebar from "../../components/sidebar";
import { Main } from "./styles";

export default function PrivateLayout() {     
  const theme = useTheme();
  const isPermanent = useMediaQuery(theme.breakpoints.up("md"));
  const [open, setOpen] = React.useState(true);
  const toggle = React.useCallback(() => setOpen(o => !o), []);

  React.useEffect(() => { setOpen(isPermanent); }, [isPermanent]);
  const contentMarginLeft = isPermanent ? (open ?"14vw" : "10vw") : 0;

  return (
    <Box  sx={{ role: "nesto",display: "flex", minHeight: "100vh" , minWidth:'100%'}} >

      <Sidebar open={open} onToggle={toggle} permanent={isPermanent} />

      <Main sx={{ ml: contentMarginLeft, width:'100%' }}>
        <Toolbar />
        <Outlet />                                
      </Main>
    </Box>
  );
}
