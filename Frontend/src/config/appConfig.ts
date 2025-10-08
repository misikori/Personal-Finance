import { AppConfigSchema, type AppConfig } from "./schema";

declare global {
  interface Window {
    __APP_CONFIG__?: Partial<AppConfig>;
  }
}

const defaults: AppConfig = AppConfigSchema.parse({ API_BASE_URL: "/api",API_BUGET_URL:"/api" });

export const loadAppConfig = (): AppConfig => {
  const raw = { ...(window.__APP_CONFIG__ ?? {}) };
  const parsed = AppConfigSchema.safeParse({ ...defaults, ...raw });
  if (!parsed.success) {
    console.error("Invalid env.js config:", parsed.error.format());
    return defaults;
  }
  return parsed.data;
};