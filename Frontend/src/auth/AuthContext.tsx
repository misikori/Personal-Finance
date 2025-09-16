import { createContext, useMemo, useState, ReactNode, useEffect } from "react";
import { mockLogin, mockLogout, getCurrentUser, isAuthenticated } from "../auth/services/authService";

export type MockUser = { id: string; email: string; roles: string[] };

type AuthContextType = {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: MockUser | null;
  login: (roles?: string[]) => Promise<void>;
  logout: () => Promise<void>;
};

export const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading, setLoading] = useState(true);
  const [user, setUser] = useState<MockUser | null>(null);
  const [authed, setAuthed] = useState(false);

  useEffect(() => {
    setUser(getCurrentUser() as MockUser | null);
    setAuthed(isAuthenticated());
    setLoading(false);
  }, []);

  const login = async (roles: string[] = ["Users"]) => {
    await mockLogin(roles);
    setUser(getCurrentUser() as MockUser | null);
    setAuthed(true);
  };

  const logout = async () => {
    await mockLogout();
    setUser(null);
    setAuthed(false);
  };

  const value = useMemo(
    () => ({
      isAuthenticated: authed,
      isLoading,
      user,
      login,
      logout,
    }),
    [authed, isLoading, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
