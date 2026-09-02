import {
  useState,
  type FormEvent,
} from "react";
import { authApi } from "../lib/api";
import type { LoginResponse } from "../types/auth";

interface Props {
  onAuthenticated: (response: LoginResponse) => void;
}

export function LoginPage({ onAuthenticated }: Props) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] =
    useState<string | null>(null);

  async function submit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!email.trim() || !password) {
      setError("Email and password are required.");
      return;
    }

    setLoading(true);

    try {
      const response =
        await authApi.login(
          email.trim(),
          password,
        );

      onAuthenticated(response);
    } catch (loginError) {
      setError(
        loginError instanceof Error
          ? loginError.message
          : "Unable to sign in.",
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="login-shell">
      <section className="login-card">
        <div className="login-brand">
          <div className="brand-mark">A</div>
          <div>
            <strong>APS AIMS</strong>
            <span>
              Asset Inventory &amp; Management System
            </span>
          </div>
        </div>

        <div className="login-copy">
          <p className="eyebrow">APS GROUP</p>
          <h1>Sign in</h1>
          <p className="muted">
            Use your authorized APS AIMS account.
          </p>
        </div>

        {error && (
          <div className="alert error">
            {error}
          </div>
        )}

        <form
          className="login-form"
          onSubmit={submit}
        >
          <label className="field">
            <span>Email</span>
            <input
              type="email"
              autoComplete="username"
              value={email}
              onChange={(event) =>
                setEmail(event.target.value)
              }
            />
          </label>

          <label className="field">
            <span>Password</span>
            <input
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
            />
          </label>

          <button
            type="submit"
            className="button primary login-button"
            disabled={loading}
          >
            {loading ? "Signing in…" : "Sign in"}
          </button>
        </form>
      </section>
    </main>
  );
}
