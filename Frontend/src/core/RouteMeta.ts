import { ComponentType, LazyExoticComponent, ReactNode } from "react";

export type Guard = "auth" | "guest" | "public";

export type RouteMeta = {
  path: string;
  Component: LazyExoticComponent<ComponentType<any>>;
  label?: string;
  icon?: ReactNode;
  roles?: string[];
  showInSidebar?: boolean;
  hideWhenAuthenticated?: boolean;
  guard?: Guard;
};