import { createContext, useMemo, useState, ReactNode, useEffect } from "react";
import { getCurrentUser, isAuthenticated as isAuthedFromStore, authStore } from "./store/authStore";
import { login as svcLogin, logout as svcLogout, me as svcMe, signup as svcSignup } from "./services/authService";

export type User = { id: string; email: string; roles: string[] };

type AuthContextType = {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
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

  useEffect(() => {
    const unsub = authStore.subscribe((s) => {
      setUser(s.user);
      setAuthed(!!s.accessToken);
    });

    (async () => {
      try {
        if (isAuthedFromStore()) await svcMe();
      } catch {} finally {
        setLoading(false);
      }
    })();

    return () => {                // return void cleanup
      unsub();                    // ignore boolean result
    };
  }, []);

  const login = async (email: string, password: string) => {
    await svcLogin({ email, password });
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
