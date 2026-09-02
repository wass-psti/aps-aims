import type {
  AuthenticatedUser,
  LoginResponse,
} from "./types/auth";
import { UserManagement } from "./components/UserManagement";
import { LoginPage } from "./components/LoginPage";
import {
  useEffect,
  useState,
} from "react";
import "./App.css";
import "./v0.5.css";
import "./v0.6.css";
import "./v0.6-employee.css";
import "./v0.7.css";
import "./v0.8.css";
import "./v0.8-fullscreen.css";
import "./v0.8-placeholder.css";
import "./v0.9.css";
import "./v1.0-auth.css";
import "./v1.0-audit.css";
import "./v1.0-user-edit.css";
import { AssetLookup } from "./components/AssetLookup";
import { AssetRegistrationForm } from "./components/AssetRegistrationForm";
import { AssetRegistry } from "./components/AssetRegistry";
import { EmployeeManagement } from "./components/EmployeeManagement";
import { ReportsPage } from "./components/ReportsPage";
import { InventoryCampaignsPage } from "./components/InventoryCampaignsPage";
import { IncidentsPage } from "./components/IncidentsPage";
import { AuditLogsPage } from "./components/AuditLogsPage";
import { canAccessView } from "./lib/permissions";

type View =
  | "registry"
  | "register"
  | "identify"
  | "employees"
  | "inventory"
  | "incidents"
  | "reports"
  | "users"
  | "audit";

const VIEW_STORAGE_KEY =
  "aps-aims-active-view";

const IS_WORKSPACE_MODE =
  import.meta.env.VITE_WORKSPACE_MODE ===
  "true";

const WORKSPACE_USER: AuthenticatedUser = {
  id: "00000000-0000-0000-0000-000000000001",
  email: "workspace@local",
  displayName: "Workspace User",
  role: "Administrator",
};

const isView = (
  value: string | null,
): value is View =>
  value === "registry" ||
  value === "register" ||
  value === "identify" ||
  value === "employees" ||
  value === "inventory" ||
  value === "incidents" ||
  value === "reports" ||
  value === "users" ||
  value === "audit";

function getInitialView(): View {
  const savedView =
    window.localStorage.getItem(
      VIEW_STORAGE_KEY,
    );

  return isView(savedView)
    ? savedView
    : "registry";
}

function getStoredUser(): AuthenticatedUser | null {
  const raw =
    localStorage.getItem(
      "aps-aims-auth-user",
    );

  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(
      raw,
    ) as AuthenticatedUser;
  } catch {
    localStorage.removeItem(
      "aps-aims-auth-user",
    );
    localStorage.removeItem(
      "aps-aims-access-token",
    );
    return null;
  }
}

function App() {
  /*
   * All hooks must run on every render.
   *
   * Previously, view/refresh hooks were below:
   *   if (!authUser) return <LoginPage ... />
   *
   * That meant the first unauthenticated render used fewer hooks than
   * the post-login render, causing React to fail until a full refresh.
   */
  const [authUser, setAuthUser] =
    useState<AuthenticatedUser | null>(
      () =>
        IS_WORKSPACE_MODE
          ? WORKSPACE_USER
          : getStoredUser(),
    );

  const [view, setView] =
    useState<View>(getInitialView);

  const [
    refreshToken,
    setRefreshToken,
  ] = useState(0);

  useEffect(() => {
    if (IS_WORKSPACE_MODE) {
      localStorage.removeItem(
        "aps-aims-access-token",
      );
      localStorage.removeItem(
        "aps-aims-auth-user",
      );

      return;
    }

    const unauthorized = () => {
      setAuthUser(null);
    };

    window.addEventListener(
      "aps-aims-unauthorized",
      unauthorized,
    );

    return () =>
      window.removeEventListener(
        "aps-aims-unauthorized",
        unauthorized,
      );
  }, []);

  useEffect(() => {
    window.localStorage.setItem(
      VIEW_STORAGE_KEY,
      view,
    );
  }, [view]);

  useEffect(() => {
    if (
      authUser &&
      !canAccessView(
        activeUser.role,
        view,
      )
    ) {
      setView("registry");
    }
  }, [authUser, view]);

  function completeLogin(
    response: LoginResponse,
  ) {
    localStorage.setItem(
      "aps-aims-access-token",
      response.accessToken,
    );
    localStorage.setItem(
      "aps-aims-auth-user",
      JSON.stringify(response.user),
    );

    setAuthUser(response.user);

    if (
      !canAccessView(
        response.user.role,
        view,
      )
    ) {
      setView("registry");
    }
  }

  function logout() {
    localStorage.removeItem(
      "aps-aims-access-token",
    );
    localStorage.removeItem(
      "aps-aims-auth-user",
    );
    setAuthUser(null);
  }

  if (!authUser && !IS_WORKSPACE_MODE) {
    return (
      <LoginPage
        onAuthenticated={completeLogin}
      />
    );
  }

  const activeUser =
    authUser ?? WORKSPACE_USER;

  const pageTitle = {
    registry: "Asset Registry",
    register: "New Asset Registration",
    identify: "Scan / Lookup",
    employees: "Employees",
    inventory: "Physical Inventory",
    incidents: "Incidents",
    reports: "Reports",
    users: "Users & Roles",
    audit: "Audit Logs",
  }[view];

  const allNavigationItems: Array<
    readonly [View, string]
  > = [
    ["registry", "Asset Registry"],
    ["register", "Register Asset"],
    ["identify", "Scan / Lookup"],
    ["employees", "Employees"],
    ["inventory", "Inventory"],
    ["incidents", "Incidents"],
    ["reports", "Reports"],
    ["users", "Users & Roles"],
    ["audit", "Audit Logs"],
  ];

  const navigationItems =
    allNavigationItems.filter(
      ([key]) =>
        (!IS_WORKSPACE_MODE ||
          key !== "users") &&
        canAccessView(
          activeUser.role,
          key,
        ),
    );

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">
          <div className="brand-mark">
            A
          </div>

          <div>
            <strong>APS AIMS</strong>
            <span>
              Asset Inventory &amp;
              Management System
            </span>
          </div>
        </div>

        <nav
          className="nav-tabs"
          aria-label="Primary navigation"
        >
          {navigationItems.map(
            ([key, label]) => (
              <button
                key={key}
                type="button"
                className={
                  view === key
                    ? "active"
                    : ""
                }
                onClick={() =>
                  setView(key)
                }
              >
                {label}
              </button>
            ),
          )}
        </nav>
      </header>

      <main>
        <div className="page-heading">
          <div>
            <p className="eyebrow">
              APS Group
            </p>
            <h1>{pageTitle}</h1>
          </div>

          <div className="auth-session">
            {IS_WORKSPACE_MODE ? (
              <span className="auth-user">
                Workspace Access
              </span>
            ) : (
              <>
                <span className="auth-user">
                  {activeUser.displayName} ·{" "}
                  {activeUser.role}
                </span>

                <button
                  type="button"
                  className="button secondary compact"
                  onClick={logout}
                >
                  Sign out
                </button>
              </>
            )}

            <span className="version-chip">
              v1.0.0
            </span>
          </div>
        </div>

        {view === "registry" && (
          <AssetRegistry
            refreshToken={refreshToken}
          />
        )}

        {view === "register" && (
          <AssetRegistrationForm
            onCreated={() => {
              setRefreshToken(
                (current) =>
                  current + 1,
              );
              setView("registry");
            }}
          />
        )}

        {view === "identify" && (
          <AssetLookup />
        )}

        {view === "employees" && (
          <EmployeeManagement />
        )}

        {view === "inventory" && (
          <InventoryCampaignsPage
            role={activeUser.role}
          />
        )}

        {view === "incidents" && (
          <IncidentsPage
            role={activeUser.role}
          />
        )}

        {view === "reports" && (
          <ReportsPage />
        )}

        {!IS_WORKSPACE_MODE &&
          view === "users" &&
          activeUser.role ===
            "Administrator" && (
            <UserManagement />
          )}

        {view === "audit" &&
          activeUser.role ===
            "Administrator" && (
            <AuditLogsPage />
          )}
      </main>
    </div>
  );
}

export default App;
