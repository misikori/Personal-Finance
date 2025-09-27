import React, { createContext, useContext, useMemo } from "react";
import { loadAppConfig } from "./appConfig";
import type { AppConfig } from "./schema";

const ConfigContext = createContext<AppConfig | null>(null);

export const ConfigProvider: React.FC<React.PropsWithChildren> = ({ children }) => {
  const value = useMemo(() => loadAppConfig(), []);
  return <ConfigContext.Provider value={value}>{children}</ConfigContext.Provider>;
};

export const useAppConfig = (): AppConfig => {
  const ctx = useContext(ConfigContext);
  if (!ctx) throw new Error("useAppConfig must be used within <ConfigProvider>");
  return ctx;
};