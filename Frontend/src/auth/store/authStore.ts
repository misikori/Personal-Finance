import { decodeJwt, getEmailFromPayload, getNameFromPayload, getRolesFromPayload } from "../jwt";

export type AuthSnapshot = {
  accessToken: string | null;
  refreshToken: string | null;
  user: { id: string; email: string; username: string, roles: string[] } | null;
};

const STORAGE_KEY = "auth:snapshot";

class AuthStore {
  private state: AuthSnapshot = { accessToken: null, refreshToken: null, user: null };
  private listeners = new Set<(s: AuthSnapshot) => void>();

  constructor() {
    this.rehydrateFromStorage();

    // Keep in sync if another tab changes it (or some tools fire a storage event)
    if (typeof window !== "undefined") {
      window.addEventListener("storage", (e) => {
        if (e.key === STORAGE_KEY) {
          this.rehydrateFromStorage();
        }
      });
    }
  }

  private persist() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.state));
    this.emit();
  }

  private emit() {
    for (const l of this.listeners) l(this.getSnapshot());
  }

  subscribe(listener: (s: AuthSnapshot) => void) {
    this.listeners.add(listener);
    return () => this.listeners.delete(listener);
  }

  getSnapshot(): AuthSnapshot {
    return { ...this.state };
  }

  setTokens(accessToken: string | null, refreshToken?: string | null) {
    this.state.accessToken = accessToken ?? null;
    if (typeof refreshToken !== "undefined") this.state.refreshToken = refreshToken ?? null;
    this.persist();
  }

  setUser(user: AuthSnapshot["user"]) {
    this.state.user = user ?? null;
    this.persist();
  }

  clear() {
    this.state = { accessToken: null, refreshToken: null, user: null };
    localStorage.removeItem(STORAGE_KEY);
    this.emit();
  }
  rehydrateFromStorage() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      this.state = raw ? JSON.parse(raw) : { accessToken: null, refreshToken: null, user: null };
    } catch {
      this.state = { accessToken: null, refreshToken: null, user: null };
    }
    this.emit();
  }
  

  get accessToken()  { return this.state.accessToken; }
  get refreshToken() { return this.state.refreshToken; }
  get user()         { return this.state.user; }
}

export const authStore = new AuthStore();

// Helpers
export const isAuthenticated = () => !!authStore.accessToken;
export const getAccessToken = () => authStore.accessToken;
export const getRefreshToken = () => authStore.refreshToken;
export const getCurrentUser  = () => authStore.user;

export function setTokensAndUserFromToken(accessToken: string | null, refreshToken: string | null) {
  authStore.setTokens(accessToken, refreshToken);
  const payload = decodeJwt(accessToken || "");
  if (payload) {
    const roles = getRolesFromPayload(payload);
    const email = getEmailFromPayload(payload) ?? "";
    const name  = getNameFromPayload(payload) ?? email ?? "";
    const id = payload.sub ?? payload.sid ?? email ?? name ?? crypto.randomUUID();

    authStore.setUser({ id: String(id), email: String(email),username:name,  roles });
  }
}