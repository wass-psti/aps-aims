import { useState } from "react";
import "./App.css";
import "./v0.5.css";
import { AssetRegistrationForm } from "./components/AssetRegistrationForm";
import { AssetRegistry } from "./components/AssetRegistry";
import { EmployeeManagement } from "./components/EmployeeManagement";

type View = "registry" | "register" | "employees";

function App() {
  const [view, setView] = useState<View>("registry");
  const [refreshToken, setRefreshToken] = useState(0);

  const pageTitle = {
    registry: "Asset Registry",
    register: "New Asset Registration",
    employees: "Employees",
  }[view];

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">
          <div className="brand-mark">A</div>
          <div>
            <strong>APS AIMS</strong>
            <span>Asset Inventory & Management System</span>
          </div>
        </div>

        <nav className="nav-tabs" aria-label="Primary navigation">
          <button
            type="button"
            className={view === "registry" ? "active" : ""}
            onClick={() => setView("registry")}
          >
            Asset Registry
          </button>

          <button
            type="button"
            className={view === "register" ? "active" : ""}
            onClick={() => setView("register")}
          >
            Register Asset
          </button>

          <button
            type="button"
            className={view === "employees" ? "active" : ""}
            onClick={() => setView("employees")}
          >
            Employees
          </button>
        </nav>
      </header>

      <main>
        <div className="page-heading">
          <div>
            <p className="eyebrow">APS Group</p>
            <h1>{pageTitle}</h1>
          </div>

          <div className="version-chip">v0.5.0</div>
        </div>

        {view === "registry" && (
          <AssetRegistry refreshToken={refreshToken} />
        )}

        {view === "register" && (
          <AssetRegistrationForm
            onCreated={() => {
              setRefreshToken((current) => current + 1);
              setView("registry");
            }}
          />
        )}

        {view === "employees" && <EmployeeManagement />}
      </main>
    </div>
  );
}

export default App;
