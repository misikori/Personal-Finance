import { styled } from "@mui/material/styles";
import { Box, Drawer, Toolbar } from "@mui/material";

export const MiniSpacer = styled("div")({
  width: "var(--drawer-mini)",
  flexShrink: 0,
});

export const BrandTitle = styled(Box)({
  fontWeight: 600,
  whiteSpace: "nowrap",
  overflow: "hidden",
  textOverflow: "ellipsis",
  transition: "opacity 120ms",
});

export const HeaderBar = styled(Toolbar)({
  justifyContent: "space-between",
});

export const PermanentDrawer = styled(Drawer)({ 
  "& .MuiDrawer-paper": {
    overflowX: "hidden",
    whiteSpace: "nowrap",
    role: "permanent"
  },
});

export const TemporaryDrawer = styled(Drawer)({
  "& .MuiDrawer-paper": {
    overflowX: "hidden",
    whiteSpace: "nowrap",
  },
});
export const NavItemWrapper = styled("div")<{"data-open"?: string}>(({ theme, ...props }) => {
  const open = props["data-open"] !== "false";
  return {
    "& .MuiListItemButton-root": {
      paddingLeft: open ? theme.spacing(2) : theme.spacing(1.25),
      paddingRight: open ? theme.spacing(2) : theme.spacing(1.25),
      justifyContent: open ? "flex-start" : "center",
    },
    "& .MuiListItemIcon-root": {
      minWidth: 0,
      marginRight: open ? theme.spacing(2) : 0,
      justifyContent: "center",
    },
    "& .MuiListItemText-root": {
      opacity: open ? 1 : 0,
      transition: "opacity 120ms",
    },
  };
});

export const Footer = styled(Box)(({ theme }) => ({
  fontSize: 12,
  color: theme.palette.text.secondary,
}));
