import { Fragment, useMemo } from "react";
import {
  Box, Divider, IconButton, List, ListItemButton, ListItemIcon, ListItemText, Tooltip
} from "@mui/material";
import MenuOpenIcon from "@mui/icons-material/MenuOpen";
import MenuIcon from "@mui/icons-material/Menu";
import { NavLink, useLocation } from "react-router-dom";
import type { SidebarProps } from "./props";
import {
  BrandTitle, Footer, HeaderBar, MiniSpacer, PermanentDrawer, TemporaryDrawer, NavItemWrapper
} from "./styles";
import { USER_ROUTES } from "../../core/RoutesConfig";

export default function Sidebar({ open, onToggle, permanent }: SidebarProps) {
  console.log(open)
  const location = useLocation();
  const items = useMemo(() => USER_ROUTES.filter(r => r.showInSidebar), []);

  const content = (
    <Box role="navigation" aria-label="Primary" sx={{ height: "100%", display: "flex", flexDirection: "column" }}>
      <HeaderBar>
        <BrandTitle aria-hidden={!open} sx={{ opacity: open ? 1 : 0 }}>App</BrandTitle>
        <IconButton onClick={onToggle} aria-label={open ? "Collapse sidebar" : "Expand sidebar"} edge="end">
          {open ? <MenuOpenIcon /> : <MenuIcon />}
        </IconButton>
      </HeaderBar>


      <List sx={{ p: 0, flex: 1 }}>
        {items.map((route) => (
          <Tooltip
            key={route.path}
            title={open ? "" : (route.label || "")}
            placement="right"
            enterDelay={600}
            disableHoverListener={open}
          >
            <NavItemWrapper data-open={open ? "true" : "false"}>
              <ListItemButton
                component={NavLink}
                to={route.path}
                onClick={!permanent ? onToggle : undefined}
                sx={{
                  "&.active": (theme) => ({
                    backgroundColor: theme.palette.action.selected,
                  }),
                }}
                aria-current={location.pathname.startsWith(route.path) ? "page" : undefined}
              >
                {route.icon && <ListItemIcon>{route.icon}</ListItemIcon>}
                <ListItemText primary={route.label} />
              </ListItemButton>
            </NavItemWrapper>
          </Tooltip>
        ))}
      </List>

      <Divider />
      <Footer sx={{ p: open ? 2 : 1, textAlign: open ? "left" : "center" }}>
        {open ? "v0.1.0" : "v0.1"}
      </Footer>
    </Box>
  );

if (permanent) {
  return (
    <Fragment>
      <PermanentDrawer
        variant="permanent"
        open
        slotProps={{
          paper: {
            sx: {
              width: open ? "22vw" : "7vw",   // <-- directly control width here
              transition: (theme) =>
                theme.transitions.create("width", {
                  easing: theme.transitions.easing.sharp,
                  duration: theme.transitions.duration.shortest,
                }),
              overflowX: "hidden",
              whiteSpace: "nowrap",
            },
          },
        }}
      >
        {content}
      </PermanentDrawer>
      {!open && <MiniSpacer aria-hidden="true" />}
    </Fragment>
  );
}

return (
  <TemporaryDrawer
    variant="temporary"
    open={open}
    onClose={onToggle}
    ModalProps={{ keepMounted: true }}
    slotProps={{
      paper: {
        sx: {
          width: "22vw", 
          transition: (theme) =>
            theme.transitions.create("width", {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.shortest,
            }),
          overflowX: "hidden",
          whiteSpace: "nowrap",
        },
      },
    }}
  >
    {content}
  </TemporaryDrawer>
);
}
