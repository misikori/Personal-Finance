import { lazy } from "react";
import { RouteMeta } from "./RouteMeta";
import SpaceDashboardIcon from "@mui/icons-material/SpaceDashboard";
import { AirlineSeatFlatRounded } from "@mui/icons-material"


export const ROUTES = {
  PUBLIC: {
    LANDING: "/",
    LOGIN: "/login",
    SIGNUP: "/signup",
  },
  PRIVATE: {
    DASHBOARD: "/dashboard",
    PORTFOLIO: "/portfolio"
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
    roles: ["Users"],
    label: "Dashboard",
    icon: <SpaceDashboardIcon />,
    showInSidebar: true,
  },
  {
    path:ROUTES.PRIVATE.PORTFOLIO,
    Component: lazy(() => import("../pages/LandingPage")),
    guard: "auth",
    roles: ["Users"],
    label: "Portfolio",
    icon: <AirlineSeatFlatRounded />,
    showInSidebar: true,
  }
];

