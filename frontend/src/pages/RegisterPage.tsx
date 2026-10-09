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
        } catch (err) {
            if (axios.isAxiosError(err) && Array.isArray(err.response?.data)) {
                setError(err.response.data.join(" "));
            } else {
                setError("Registration failed.");
            }
        }
    }

    return (
        <form onSubmit={handleSubmit}>
            <h1>Create Account</h1>
            <input value={displayName} onChange={(e) => setDisplayName(e.target.value)} placeholder="Display name" required />
            <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" required />
            <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" required />
            <ErrorMessage message={error} />
            <button type="submit">Register</button>
            <p>Already have an account? <Link to="/login">Log in</Link></p>
        </form>
    );
}
