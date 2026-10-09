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
        <form onSubmit={handleSubmit}>
            <h1>Log in</h1>

            <div>
                <label htmlFor="email">Email</label>
                <input
                    id="email"
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />
            </div>

            <div>
                <label htmlFor="password">Password</label>
                <input
                    id="password"
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                />
            </div>

            <ErrorMessage message={error} />

            <button type="submit">Log in</button>

            <p>Don't have an account? <Link to="/register">Register</Link></p>
        </form>
    );
}
