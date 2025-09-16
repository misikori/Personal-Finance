import { createContext, useMemo, useState, ReactNode } from "react";

export type MockUser = {
  id: string;
  email: string;
  roles: string[];
};

type AuthContextType = {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: MockUser | null;
  login: (roles?: string[]) => void;
  logout: () => void;
};

export const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [isLoading] = useState(false);
  const [user, setUser] = useState<MockUser | null>(null);

  const value = useMemo(
    () => ({
      isAuthenticated: !!user,
      isLoading,
      user,
      login: (roles: string[] = ["Users"]) =>
        setUser({ id: "1", email: "demo@demo.test", roles }),
      logout: () => setUser(null),
    }),
    [user, isLoading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
