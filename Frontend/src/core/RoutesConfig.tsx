import { lazy } from "react";
import { RouteMeta } from "./RouteMeta";
import SpaceDashboardIcon from "@mui/icons-material/SpaceDashboard";
import ReceiptLongIcon from "@mui/icons-material/ReceiptLong";
import InventoryIcon from "@mui/icons-material/Inventory"; 


export const ROUTES = {
  PUBLIC: {
    LANDING: "/",
    LOGIN: "/login",
    SIGNUP: "/signup",
  },
  PRIVATE: {
    DASHBOARD: "/dashboard",
    PORTFOLIO: "/portfolio",
    TRANSACTIONS: "/transactions",
  },
};

export const PUBLIC_ROUTES: RouteMeta[] = [
  {
    path: ROUTES.PUBLIC.LANDING,
    Component: lazy(() => import("../pages/LandingPage")),
    guard: "public",
    label: "Home",
  },
  {
    path: ROUTES.PUBLIC.LOGIN,
    Component: lazy(() => import("../auth/components/Login")),
    guard: "guest",
    label: "Login",
  },
  {
    path: ROUTES.PUBLIC.SIGNUP,
    Component: lazy(() => import("../auth/components/Signup")),
    guard: "guest",
    label: "Sign Up",
  },
];

export const USER_ROUTES: RouteMeta[] = [
  {
    path: ROUTES.PRIVATE.DASHBOARD,
    Component: lazy(() => import("../pages/dashboard")),
    guard: "auth",
    roles: ["User"],
    label: "Dashboard",
    icon: <SpaceDashboardIcon />,
    showInSidebar: true,
  },
  {
    path: ROUTES.PRIVATE.TRANSACTIONS,
    Component: lazy(() => import("../pages/transactions")),
    guard: "auth",
    roles: ["User"],
    label: "Transactions",
    icon: <ReceiptLongIcon />,
    showInSidebar: true,
  },
  {
    path: ROUTES.PRIVATE.PORTFOLIO,
    Component: lazy(() => import("../pages/portfolio")),
    guard: "auth",
    roles: ["User"],
    label: "Portfolio",
    icon: <InventoryIcon />,
    showInSidebar: true,
  },
];

