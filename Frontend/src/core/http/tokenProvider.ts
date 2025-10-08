import { authStore, getRefreshToken } from "../../auth/store/authStore";
import { loadAppConfig } from "../../config/appConfig";
import { getTokenExpiry } from "../../auth/jwt";

const cfg = loadAppConfig();

let isRefreshing = false;
let waiters: Array<() => void> = [];
const queue = () => new Promise<void>(resolve => waiters.push(resolve));
const flush = () => { waiters.forEach(fn => fn()); waiters = []; };

function isExpiringSoon(token?: string | null, skewSeconds = 30): boolean {
  const exp = getTokenExpiry(token);
  if (!exp) return false;
  const now = Math.floor(Date.now() / 1000);
  return exp <= now + skewSeconds;
}


export async function refreshTokensOnce(): Promise<void> {
  if (isRefreshing) { await queue(); return; }
  isRefreshing = true;
  try {
    const rt = getRefreshToken();
    if (!rt) throw new Error("No refresh token");

    const res = await fetch(`${cfg.API_BASE_URL}/api/v1/Authentication/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ refreshToken: rt }),
    });

    const text = await res.text();
    if (!res.ok) {
      let msg = `HTTP ${res.status} ${res.statusText}`;
      try {
        const prob = JSON.parse(text);
        msg = prob?.title ?? prob?.detail ?? msg;
      } catch {}
      throw new Error(msg);
    }

    const data = JSON.parse(text || "{}");
    if (!data?.accessToken) throw new Error("Refresh response missing accessToken");
    authStore.setTokens(data.accessToken, data.refreshToken ?? rt);
  } finally {
    isRefreshing = false;
    flush();
  }
}

export async function ensureAccessToken(): Promise<string | null> {
  const token = authStore.accessToken;
  if (!isExpiringSoon(token, 30)) return token ?? null;

  try {
    await refreshTokensOnce();
    return authStore.accessToken;
  } catch {
    authStore.clear();
    return null;
  }
}
