import { authStore, getAccessToken, getRefreshToken } from "../auth/store/authStore";
import { API_BASE_URL } from "../config/env";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
type RequestOptions = { method?: HttpMethod; headers?: Record<string, string>; body?: any; signal?: AbortSignal; credentials?: RequestCredentials };


let isRefreshing = false;
let refreshWaiters: Array<() => void> = [];

function queueUntilRefreshed() {
  return new Promise<void>((resolve) => refreshWaiters.push(resolve));
}

function flushRefreshQueue() {
  refreshWaiters.forEach(fn => fn());
  refreshWaiters = [];
}

function isJsonContentType(ct: string | null) {
  return !!ct && ct.toLowerCase().includes("application/json");
}

function safeJson(text: string) {
  try { return JSON.parse(text); } catch { return undefined; }
}
async function refreshTokensOnce() {
  if (isRefreshing) { await queueUntilRefreshed(); return; }
  isRefreshing = true;
  try {
    const rt = getRefreshToken();
    if (!rt) throw new Error("No refresh token");

    const url = `${API_BASE_URL}/api/v1/Authentication/refresh`;
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/json", "Accept": "application/json" },
      body: JSON.stringify({ refreshToken: rt }),
    });

    const text = await res.text();
    if (!res.ok) {
      const prob = isJsonContentType(res.headers.get("content-type")) ? safeJson(text) : undefined;
      const msg = prob?.title ?? prob?.detail ?? `HTTP ${res.status} ${res.statusText}`;
      throw new Error(msg);
    }
    const data = safeJson(text);
    if (!data?.accessToken) throw new Error("Refresh response missing accessToken");
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

  const url = path.startsWith("http") ? path : `${API_BASE_URL}${path}`;
  const method = options.method ?? "GET";

  const headers: Record<string, string> = {
    Accept: "application/json, text/plain, */*",
    ...(options.headers ?? {})
  };

  let bodyInit: BodyInit | undefined;
  if (options.body instanceof FormData) {
    bodyInit = options.body as any; // let the browser set boundary
  } else if (options.body !== undefined && options.body !== null) {
    headers["Content-Type"] = headers["Content-Type"] ?? "application/json";
    bodyInit = JSON.stringify(options.body);
  }

  const at = getAccessToken();
  if (at) headers["Authorization"] = `Bearer ${at}`;

  const res = await fetch(url, {
    method,
    headers,
    body: bodyInit,
    signal: options.signal,
    credentials: options.credentials, // set to "include" if you rely on cookies
  });

  const ct = res.headers.get("content-type");
  const raw = await res.text();
  
  if (res.status === 401 && retryOn401) {
    try {
      await refreshTokensOnce();
      return apiFetch<T>(path, options, false);
    } catch {
      authStore.clear();
      const err = new Error("Unauthorized");
      (err as any).status = 401;
      throw err;
    }
  }

  if (!res.ok) {
    const problem = isJsonContentType(ct) ? safeJson(raw) : undefined;
    const err = new Error(
      problem?.title ?? problem?.detail ?? `HTTP ${res.status} ${res.statusText}`
    ) as Error & { status?: number; data?: any; body?: string; url?: string; method?: string };
    err.status = res.status;
    err.data = problem;
    err.body = raw;
    err.url = url;
    err.method = method;
    throw err;
  }

  if (res.status === 204 || raw.trim() === "") {
    return undefined as T;
  }
  if (isJsonContentType(ct)) {
    const data = safeJson(raw);
    if (data === undefined) {
      const err = new Error("Failed to parse JSON response.") as Error & { raw?: string; url?: string };
      err.raw = raw; err.url = url;
      throw err;
    }
    return data as T;
  }

  return raw as unknown as T;
}
