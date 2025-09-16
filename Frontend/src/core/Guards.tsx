import { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/hooks/useAuth";
import Loader from "../components/Loader";

export function PrivateRoute({
  children,
  roles,
}: {
  children: ReactNode;
  roles?: string[];
}) {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) return <Loader />;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0) {
    const hasRole = roles.some((r) => user?.roles?.includes(r));
    if (!hasRole) return <Navigate to="/403" replace />;
  }

  return <>{children}</>;
}

export function PublicRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return <Loader />;
  if (isAuthenticated) return <Navigate to="/" replace />;

  return <>{children}</>;
}
