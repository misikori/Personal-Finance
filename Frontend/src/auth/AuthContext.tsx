import { createContext, useMemo, useState, ReactNode, useEffect } from "react";
import { getCurrentUser, isAuthenticated as isAuthedFromStore, authStore } from "./store/authStore";
import { login as svcLogin, logout as svcLogout,signup as svcSignup } from "./services/authService";
import { decodeJwt, getEmailFromPayload, getNameFromPayload, getRolesFromPayload, isTokenExpired } from "./jwt";

export type User = { id: string; email: string; roles: string[] };

type AuthContextType = {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: User | null;
  login: (userName: string, password: string) => Promise<void>;
  signup: (dto: {
    firstName: string;
    lastName: string;
    userName: string;
    password: string;
    email: string;
    phoneNumber: string;
  }) => Promise<void>;
  logout: () => Promise<void>;
};

export const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading, setLoading] = useState(true);
  const [user, setUser] = useState<User | null>(getCurrentUser());
  const [authed, setAuthed] = useState<boolean>(isAuthedFromStore());

  const computeUserFromToken = (accessToken: string | null | undefined) => {
      const p = decodeJwt(accessToken || "");
      if (!p) return null;
      const email = getEmailFromPayload(p) ?? "";
      const name  = getNameFromPayload(p) ?? email ?? "";
      const roles = getRolesFromPayload(p);
      const id    = String(p.sub ?? p.sid ?? email ?? name);
      return { id, email, roles } as User;
  };

  useEffect(() => {
    // 1) make memory match storage
    authStore.rehydrateFromStorage();

    // 2) initialize local state synchronously from snapshot
    const snap = authStore.getSnapshot();
    setUser(snap.user ?? computeUserFromToken(snap.accessToken));
    setAuthed(!!snap.accessToken && !isTokenExpired(snap.accessToken));
    setLoading(false);

    // 3) keep in sync on store changes
    const unsub = authStore.subscribe((s) => {
      setUser(s.user ?? computeUserFromToken(s.accessToken));
      setAuthed(!!s.accessToken && !isTokenExpired(s.accessToken));
    });

    // 4) rehydrate when window regains focus (covers same-tab localStorage edits)
    const onFocus = () => authStore.rehydrateFromStorage();
    window.addEventListener("focus", onFocus);

    return () => { window.removeEventListener("focus", onFocus); unsub(); };
  }, []);


  const login = async (userName: string, password: string) => {
    await svcLogin({ userName, password });
  };

  const signup = async (dto: {
    firstName: string; lastName: string; userName: string;
    password: string; email: string; phoneNumber: string;
  }) => {
    await svcSignup(dto);
  };

  const logout = async () => {
    await svcLogout();
  };

  const value = useMemo(
    () => ({ isAuthenticated: authed, isLoading, user, login, logout, signup }),
    [authed, isLoading, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
