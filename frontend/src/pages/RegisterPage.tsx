import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { register } from "../api/auth";
import ErrorMessage from "../components/ErrorMessage";

export default function RegisterPage() {
    const [displayName, setDisplayName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();

    async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        setError(null);

        try {
            await register(email, password, displayName);
            navigate("/boards");
        } catch (err: unknown) {
            if (axios.isAxiosError(err) && Array.isArray(err.response?.data)) {
                setError(err.response.data.join(" "));
            } else {
                setError("Registration failed.");
            }
        }
    }

    return (
        <div className="flex min-h-screen items-center justify-center">
            <form
                onSubmit={handleSubmit}
                className="w-full max-w-sm space-y-4 rounded-lg bg-white p-8 shadow"
            >
                <h1 className="text-2xl font-semibold">Create Account</h1>

                <div className="space-y-1">
                    <label htmlFor="display-name" className="text-sm font-medium text-slate-700">Display Name</label>
                    <input
                        id="display-name"
                        className="input"
                        value={displayName}
                        onChange={(e) => setDisplayName(e.target.value)}
                        required
                    />
                </div>

                <div className="space-y-1">
                    <label htmlFor="email" className="text-sm font-medium text-slate-700">Email</label>
                    <input
                        id="email"
                        type="email"
                        className="input"
                        value={email} onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>

                <div className="space-y-1">
                    <label htmlFor="password" className="text-sm font-medium text-slate-700">Password</label>
                    <input
                        id="password"
                        type="password"
                        className="input"
                        value={password} onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>
                
                <ErrorMessage message={error} />

                <button type="submit" className="btn-primary w-full">Register</button>

                <p className="text-center text-sm text-slate-600">
                    Already have an account? <Link to="/login" className="text-indigo-600 hover:underline">Log in</Link>
                </p>
            </form>
        </div>
    );
}
