import { useState } from "react";
import "./App.css";
import { AssetRegistrationForm } from "./components/AssetRegistrationForm";
import { AssetRegistry } from "./components/AssetRegistry";

type View = "registry" | "register";

function App() {
  const [view, setView] = useState<View>("registry");
  const [refreshToken, setRefreshToken] = useState(0);

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
        </nav>
      </header>

      <main>
        <div className="page-heading">
          <div>
            <p className="eyebrow">APS Group</p>
            <h1>
              {view === "registry"
                ? "Asset Registry"
                : "New Asset Registration"}
            </h1>
          </div>

          <div className="version-chip">v0.3.0</div>
        </div>

        {view === "registry" ? (
          <AssetRegistry refreshToken={refreshToken} />
        ) : (
          <AssetRegistrationForm
            onCreated={() => {
              setRefreshToken((current) => current + 1);
              setView("registry");
            }}
          />
        )}
      </main>
    </div>
  );
}

export default App;
