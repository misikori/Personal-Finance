import { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/hooks/useAuth";
import Loader from "../components/Loader";
import { isTokenExpired } from "../auth/jwt";
import { authStore, getAccessToken } from "../auth/store/authStore";

export function PrivateRoute({
  children,
  roles,
}: {
  children: ReactNode;
  roles?: string[];
}) {
  const {isLoading, user } = useAuth();

  if (isLoading) return <Loader />;

  if (!localStorage.getItem("auth:snapshot") && authStore.accessToken) {
    authStore.clear();
  }
  const token = getAccessToken();
  if (!token || isTokenExpired(token)) {
    return <Navigate to="/login" replace />;
  }
    if (roles?.length) {
    const have = (user?.roles ?? []).map(r => r.toLowerCase());
    const need = roles.map(r => r.toLowerCase());
    const ok = need.some(r => have.includes(r));
    if (!ok) return <Navigate to="/403" replace />;
  }

  return <>{children}</>;
}

export function PublicRoute({ children }: { children: ReactNode }) {
  const token = getAccessToken();
  const authed = !!token && !isTokenExpired(token);

  if (authed) return <Navigate to="/" replace />;
  return <>{children}</>;
}
