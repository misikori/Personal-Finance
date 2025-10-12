import { lazy } from "react";
import { RouteMeta } from "./RouteMeta";
import SpaceDashboardIcon from "@mui/icons-material/SpaceDashboard";
import ReceiptLongIcon from "@mui/icons-material/ReceiptLong";
import InventoryIcon from "@mui/icons-material/Inventory"; 
import AccountBalanceWalletIcon from "@mui/icons-material/AccountBalanceWallet";
import TrendingUpIcon from "@mui/icons-material/TrendingUp"; 
import ReplayIcon from "@mui/icons-material/Replay";


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
    WALLETS: "/wallets",                
    RECURRING: "/recurring-transactions", 
    TRADE: "/portfolio/trade",
  },
};

export const PUBLIC_ROUTES: RouteMeta[] = [
  {
    path: ROUTES.PUBLIC.LANDING,
    Component: lazy(() => import("../pages/landingpage")),
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
  {
    path: ROUTES.PRIVATE.WALLETS,
    Component: lazy(() => import("../pages/wallets")),  
    guard: "auth",
    roles: ["User"],
    label: "Wallets",
    icon: <AccountBalanceWalletIcon />,
    showInSidebar: true,
  },
  {
    path: ROUTES.PRIVATE.RECURRING,
    Component: lazy(() => import("../pages/recurring-transactions")),
    guard: "auth",
    roles: ["User"],
    label: "Recurring",
    icon: <ReplayIcon />,
    showInSidebar: true,
  },
  {
  path: ROUTES.PRIVATE.TRADE,
  Component: lazy(() => import("../pages/trade")), // we created index.tsx
  guard: "auth",
  roles: ["User"],
  label: "Trade",
  icon: <TrendingUpIcon />,
  showInSidebar: true,
},
  
];

