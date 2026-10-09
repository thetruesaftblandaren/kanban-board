import { Link, Outlet, useNavigate } from "react-router-dom";
import { logout } from "../api/auth";

export default function Layout() {
    const navigate = useNavigate();

    async function handleLogout() {
        await logout();
        navigate("/login");
    }

    return (
        <div>
            <nav>
                <Link to="/boards">Boards</Link>
                <button type="button" onClick={handleLogout}>Log out</button>
            </nav>
            <Outlet />
        </div>
    );
}
