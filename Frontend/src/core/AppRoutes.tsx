import {
  BrowserRouter as Router,
  Routes,
  Route,
} from "react-router-dom";
import { JSX, Suspense } from "react";
import { PUBLIC_ROUTES, USER_ROUTES } from "./RoutesConfig";
import { RouteMeta } from "./RouteMeta";
import Loader from "../components/Loader";
import { PrivateRoute, PublicRoute } from "./Guards";
import Forbidden from "../components/Forbidden";
import NotFound from "../components/NotFound";
import PrivateLayout from "../layouts/PrivateLayout";
import PublicLayout from "../layouts/PublicLayout";


const wrapWithGuard = (route: RouteMeta, node: JSX.Element) => {
  if (route.guard === "auth") {
    return <PrivateRoute roles={route.roles}>{node}</PrivateRoute>;
  }
  if (route.guard === "guest") {
    return <PublicRoute>{node}</PublicRoute>;
  }
  return node; 
};

const renderRoute = (route: RouteMeta) => {
  const C = route.Component;
  const element = (
    <Suspense fallback={<Loader />}>
      <C />
    </Suspense>
  );

  return (
    <Route
      key={route.path}
      path={route.path}
      element={wrapWithGuard(route, element)}
    />
  );
};

const AppRoutes = () => {
  return (
    <Router>
      <Routes>
        <Route element={<PublicLayout />}>
          {PUBLIC_ROUTES.map(renderRoute)}
        </Route>

        <Route element={<PrivateLayout />}>
          {USER_ROUTES.map(renderRoute)}
        </Route>

        <Route path="/403" element={<Forbidden />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Router>
  );
};

export default AppRoutes;
