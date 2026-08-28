import {
  useEffect,
  useState,
  type FormEvent,
} from "react";
import { useAssetMasterData } from "../hooks/useAssetMasterData";
import { aimsApi } from "../lib/api";
import type {
  CreateEmployeeRequest,
  Employee,
} from "../types/aims";

const emptyForm = {
  employeeNumber: "",
  firstName: "",
  lastName: "",
  email: "",
  companyId: "",
  branchId: "",
  departmentId: "",
};

export function EmployeeManagement() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const masterData = useAssetMasterData(
    form.companyId,
    form.branchId,
  );

  async function loadEmployees() {
    setLoading(true);

    try {
      setEmployees(await aimsApi.getEmployees());
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load employees.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadEmployees();
  }, []);

  function updateField(key: keyof typeof form, value: string) {
    setForm((current) => {
      const next = { ...current, [key]: value };

      if (key === "companyId") {
        next.branchId = "";
        next.departmentId = "";
      }

      if (key === "branchId") {
        next.departmentId = "";
      }

      return next;
    });
  }

  async function submitEmployee(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!form.firstName.trim() || !form.lastName.trim()) {
      setError("First name and last name are required.");
      return;
    }

    const payload: CreateEmployeeRequest = {
      employeeNumber: form.employeeNumber.trim(),
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      departmentId: form.departmentId || null,
    };

    setSaving(true);
    setError(null);

    try {
      await aimsApi.createEmployee(payload);
      setForm(emptyForm);
      await loadEmployees();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to create employee.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="employee-layout">
      <section className="panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">People</p>
            <h2>Employees</h2>
            <p className="muted">
              Active employees can receive issued assets.
            </p>
          </div>
        </div>

        {error && <div className="alert error">{error}</div>}

        {loading ? (
          <div className="empty-state">Loading employees…</div>
        ) : employees.length === 0 ? (
          <div className="empty-state">No employees registered yet.</div>
        ) : (
          <div className="employee-list">
            {employees.map((employee) => (
              <div className="employee-row" key={employee.id}>
                <div>
                  <strong>{employee.displayName}</strong>
                  <span>
                    {employee.employeeNumber || "No employee number"}
                  </span>
                </div>

                <div>
                  <strong>{employee.departmentName || "No department"}</strong>
                  <span>{employee.branchName || "—"}</span>
                </div>

                <div>
                  <span>{employee.email || "No email"}</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">New Employee</p>
            <h2>Add employee</h2>
            <p className="muted">
              Create an employee record for asset custody assignment.
            </p>
          </div>
        </div>

        <form onSubmit={submitEmployee}>
          <fieldset disabled={saving}>
            <div className="employee-form">
              <label className="field">
                <span>Employee number</span>
                <input
                  value={form.employeeNumber}
                  onChange={(event) =>
                    updateField("employeeNumber", event.target.value)
                  }
                  placeholder="e.g. EMP-001"
                />
              </label>

              <label className="field">
                <span>Email</span>
                <input
                  type="email"
                  value={form.email}
                  onChange={(event) =>
                    updateField("email", event.target.value)
                  }
                />
              </label>

              <label className="field">
                <span>First name *</span>
                <input
                  value={form.firstName}
                  onChange={(event) =>
                    updateField("firstName", event.target.value)
                  }
                />
              </label>

              <label className="field">
                <span>Last name *</span>
                <input
                  value={form.lastName}
                  onChange={(event) =>
                    updateField("lastName", event.target.value)
                  }
                />
              </label>

              <label className="field">
                <span>Company</span>
                <select
                  value={form.companyId}
                  onChange={(event) =>
                    updateField("companyId", event.target.value)
                  }
                >
                  <option value="">Select company</option>
                  {masterData.companies.map((company) => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Branch</span>
                <select
                  value={form.branchId}
                  disabled={!form.companyId}
                  onChange={(event) =>
                    updateField("branchId", event.target.value)
                  }
                >
                  <option value="">Select branch</option>
                  {masterData.branches.map((branch) => (
                    <option key={branch.id} value={branch.id}>
                      {branch.name}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field field-wide">
                <span>Department</span>
                <select
                  value={form.departmentId}
                  disabled={!form.branchId}
                  onChange={(event) =>
                    updateField("departmentId", event.target.value)
                  }
                >
                  <option value="">No department</option>
                  {masterData.departments.map((department) => (
                    <option key={department.id} value={department.id}>
                      {department.name}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <div className="form-actions">
              <button type="submit" className="button primary">
                {saving ? "Adding…" : "Add employee"}
              </button>
            </div>
          </fieldset>
        </form>
      </section>
    </div>
  );
}
