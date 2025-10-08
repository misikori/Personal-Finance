export const API_BASE_URL =
  (window as any).__APP_CONFIG__?.API_BASE_URL ??
  import.meta.env.VITE_API_BASE_URL ??
  "http://localhost:5059";