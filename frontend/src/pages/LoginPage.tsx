import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/auth";
import ErrorMessage from "../components/ErrorMessage";

export default function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        setError(null);

        try {
            await login(email, password);
            navigate("/boards");
        } catch {
            setError("Invalid email or password.");
        }
    }
    
    return (
        <div className="flex min-h-screen items-center justify-center">
            <form
                onSubmit={handleSubmit}
                className="w-full max-w-sm space-y-4 rounded-lg bg-white p-8 shadow"
            >
                <h1 className="text-2xl font-semibold">Log in</h1>

                <div className="space-y-1">
                    <label htmlFor="email" className="text-sm font-medium text-slate-700">Email</label>
                    <input
                        id="email"
                        type="email"
                        className="input"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>

                <div className="space-y-1">
                    <label htmlFor="password" className="text-sm font-medium text-slate-700">Password</label>
                    <input
                        id="password"
                        type="password"
                        className="input"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>

                <ErrorMessage message={error} />

                <button type="submit" className="btn-primary w-full">Log in</button>

                <p className="text-center text-sm text-slate-600">
                    Don't have an account? <Link to="/register" className="text-indigo-600 hover:underline">Register</Link>
                </p>
            </form>
        </div>
    );
}
