import { Link, Outlet, useNavigate } from "react-router-dom";
import { logout } from "../api/auth";

export default function Layout() {
    const navigate = useNavigate();

    async function handleLogout() {
        await logout();
        navigate("/login");
    }

    return (
        <div className="min-h-screen">
            <nav className=" sticky top-0 z-50 flex items-center justify-between border-b borde-slate-200 bg-white px-6 py-3">
                <Link to="/boards" className="text-lg font-semibold text-indigo-600 hover:underline" >
                    Boards
                </Link>
                <button type="button" onClick={handleLogout} className="btn-secondary">
                    Log out
                </button>
            </nav>
            <main className="p-6">
                <Outlet />
            </main>
        </div>
    );
}
