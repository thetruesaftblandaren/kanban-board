import { Navigate, Outlet } from "react-router-dom";

export default function ProtectedRoute() {
    const hasSession = localStorage.getItem("refreshToken") !== null;
    return hasSession ? <Outlet />: <Navigate to="login" replace />
}
