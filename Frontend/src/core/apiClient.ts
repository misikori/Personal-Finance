import { authStore, getAccessToken, getRefreshToken } from "../auth/store/authStore";
import { API_BASE_URL } from "../config/env";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
type RequestOptions = { method?: HttpMethod; headers?: Record<string, string>; body?: any; signal?: AbortSignal };

let isRefreshing = false;
let refreshWaiters: Array<() => void> = [];

function queueUntilRefreshed() {
  return new Promise<void>((resolve) => refreshWaiters.push(resolve));
}

function flushRefreshQueue() {
  refreshWaiters.forEach(fn => fn());
  refreshWaiters = [];
}

async function refreshTokensOnce() {
  if (isRefreshing) {
    await queueUntilRefreshed();
    return;
  }
  isRefreshing = true;
  try {
    const rt = getRefreshToken();
    if (!rt) throw new Error("No refresh token");
    const res = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken: rt })
    });
    if (!res.ok) throw new Error("Refresh failed");
    const data = await res.json();
    authStore.setTokens(data.accessToken, data.refreshToken ?? getRefreshToken());
  } finally {
    isRefreshing = false;
    flushRefreshQueue();
  }
}

export async function apiFetch<T>(
  path: string,
  options: RequestOptions = {},
  retryOn401 = true
): Promise<T> {
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers ?? {})
  };

  const at = getAccessToken();
  if (at) headers["Authorization"] = `Bearer ${at}`;

  const resp = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: options.body ? JSON.stringify(options.body) : undefined,
    signal: options.signal
  });

  if (resp.status === 401 && retryOn401) {
    try {
      await refreshTokensOnce();
      return apiFetch<T>(path, options, false);
    } catch {
      authStore.clear();
      throw new Error("Unauthorized");
    }
  }

  if (!resp.ok) {
    const text = await resp.text().catch(() => "");
    throw new Error(text || `HTTP ${resp.status}`);
  }

  return resp.json() as Promise<T>;
}
