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
  UpdateEmployeeRequest,
} from "../types/aims";

const emptyForm = {
  employeeNumber: "",
  firstName: "",
  lastName: "",
  email: "",
  companyId: "",
  branchId: "",
  departmentId: "",
  isActive: true,
};

type EmployeeForm = typeof emptyForm;

export function EmployeeManagement() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [form, setForm] = useState<EmployeeForm>(emptyForm);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const masterData = useAssetMasterData(
    form.companyId,
    form.branchId,
  );

  async function loadEmployees() {
    setLoading(true);

    try {
      setEmployees(await aimsApi.getEmployees(true));
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

  function updateField<K extends keyof EmployeeForm>(
    key: K,
    value: EmployeeForm[K],
  ) {
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

  function clearForm() {
    setEditingId(null);
    setForm(emptyForm);
    setError(null);
  }

  function editEmployee(employee: Employee) {
    setEditingId(employee.id);
    setForm({
      employeeNumber: employee.employeeNumber ?? "",
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email ?? "",
      companyId: employee.companyId ?? "",
      branchId: employee.branchId ?? "",
      departmentId: employee.departmentId ?? "",
      isActive: employee.isActive,
    });
    setError(null);
  }

  async function submitEmployee(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!form.firstName.trim() || !form.lastName.trim()) {
      setError("First name and last name are required.");
      return;
    }

    setSaving(true);
    setError(null);

    try {
      if (editingId) {
        const payload: UpdateEmployeeRequest = {
          employeeNumber: form.employeeNumber.trim(),
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          email: form.email.trim(),
          departmentId: form.departmentId || null,
          isActive: form.isActive,
        };

        await aimsApi.updateEmployee(editingId, payload);
      } else {
        const payload: CreateEmployeeRequest = {
          employeeNumber: form.employeeNumber.trim(),
          firstName: form.firstName.trim(),
          lastName: form.lastName.trim(),
          email: form.email.trim(),
          departmentId: form.departmentId || null,
        };

        await aimsApi.createEmployee(payload);
      }

      clearForm();
      await loadEmployees();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to save employee.",
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteEmployee(employee: Employee) {
    const confirmed = window.confirm(
      `Permanently delete ${employee.displayName} from the database?`,
    );

    if (!confirmed) return;

    setDeletingId(employee.id);
    setError(null);

    try {
      await aimsApi.deleteEmployee(employee.id);

      if (editingId === employee.id) {
        clearForm();
      }

      await loadEmployees();
    } catch (deleteError) {
      setError(
        deleteError instanceof Error
          ? deleteError.message
          : "Unable to delete employee.",
      );
    } finally {
      setDeletingId(null);
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
              Edit employee records or permanently delete unused records.
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
              <div className="employee-row employee-row-actions" key={employee.id}>
                <div>
                  <div className="employee-name-line">
                    <strong>{employee.displayName}</strong>
                    <span
                      className={
                        employee.isActive
                          ? "employee-status active"
                          : "employee-status inactive"
                      }
                    >
                      {employee.isActive ? "Active" : "Inactive"}
                    </span>
                  </div>
                  <span>{employee.employeeNumber || "No employee number"}</span>
                </div>

                <div>
                  <strong>{employee.departmentName || "No department"}</strong>
                  <span>{employee.branchName || "—"}</span>
                </div>

                <div>
                  <span>{employee.email || "No email"}</span>
                </div>

                <div className="employee-actions">
                  <button
                    type="button"
                    className="button secondary compact"
                    onClick={() => editEmployee(employee)}
                  >
                    Edit
                  </button>

                  <button
                    type="button"
                    className="button danger compact"
                    disabled={deletingId === employee.id}
                    onClick={() => deleteEmployee(employee)}
                  >
                    {deletingId === employee.id ? "Deleting…" : "Delete"}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <section className="panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">
              {editingId ? "Edit Employee" : "New Employee"}
            </p>
            <h2>{editingId ? "Edit employee" : "Add employee"}</h2>
            <p className="muted">
              {editingId
                ? "Update the selected employee record."
                : "Create an employee record for asset custody assignment."}
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

              {editingId && (
                <label className="employee-active-toggle field-wide">
                  <input
                    type="checkbox"
                    checked={form.isActive}
                    onChange={(event) =>
                      updateField("isActive", event.target.checked)
                    }
                  />
                  <span>Employee is active</span>
                </label>
              )}
            </div>

            <div className="form-actions employee-form-actions">
              {editingId && (
                <button
                  type="button"
                  className="button secondary"
                  onClick={clearForm}
                >
                  Cancel edit
                </button>
              )}

              <button type="submit" className="button primary">
                {saving
                  ? "Saving…"
                  : editingId
                    ? "Save changes"
                    : "Add employee"}
              </button>
            </div>
          </fieldset>
        </form>
      </section>
    </div>
  );
}
