import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import { authApi } from "../lib/api";
import type {
  AimsRole,
  ApplicationUser,
  AuthenticatedUser,
} from "../types/auth";

const ROLES: AimsRole[] = [
  "Administrator",
  "AssetManager",
  "Custodian",
  "Viewer",
];

type EditState = {
  userId: string;
  firstName: string;
  lastName: string;
  role: AimsRole;
};

function getCurrentUserId() {
  const raw = localStorage.getItem(
    "aps-aims-auth-user",
  );

  if (!raw) {
    return null;
  }

  try {
    return (
      JSON.parse(raw) as AuthenticatedUser
    ).id;
  } catch {
    return null;
  }
}

export function UserManagement() {
  const [users, setUsers] =
    useState<ApplicationUser[]>([]);

  const [email, setEmail] = useState("");
  const [password, setPassword] =
    useState("");
  const [firstName, setFirstName] =
    useState("");
  const [lastName, setLastName] =
    useState("");
  const [role, setRole] =
    useState<AimsRole>("Viewer");

  const [editing, setEditing] =
    useState<EditState | null>(null);

  const [savingUserId, setSavingUserId] =
    useState<string | null>(null);

  const [error, setError] =
    useState<string | null>(null);

  const [success, setSuccess] =
    useState<string | null>(null);

  const currentUserId = useMemo(
    getCurrentUserId,
    [],
  );

  async function load() {
    try {
      setUsers(await authApi.getUsers());
      setError(null);
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load users.",
      );
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function create(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setSuccess(null);

    try {
      await authApi.createUser({
        email: email.trim(),
        password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        role,
      });

      setEmail("");
      setPassword("");
      setFirstName("");
      setLastName("");
      setRole("Viewer");

      setSuccess(
        "User account created successfully.",
      );

      await load();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to create user.",
      );
    }
  }

  function beginEdit(
    user: ApplicationUser,
  ) {
    setError(null);
    setSuccess(null);

    setEditing({
      userId: user.id,
      firstName: user.firstName,
      lastName: user.lastName,
      role: user.role,
    });
  }

  function cancelEdit() {
    setEditing(null);
    setError(null);
  }

  async function saveEdit(
    user: ApplicationUser,
  ) {
    if (
      !editing ||
      editing.userId !== user.id
    ) {
      return;
    }

    if (
      !editing.firstName.trim() ||
      !editing.lastName.trim()
    ) {
      setError(
        "First name and last name are required.",
      );
      return;
    }

    setSavingUserId(user.id);
    setError(null);
    setSuccess(null);

    try {
      await authApi.updateUser(
        user.id,
        {
          firstName:
            editing.firstName.trim(),
          lastName:
            editing.lastName.trim(),
          role: editing.role,
          isActive: user.isActive,
        },
      );

      setEditing(null);
      setSuccess(
        `${user.email} was updated successfully.`,
      );

      await load();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to update user.",
      );
    } finally {
      setSavingUserId(null);
    }
  }

  async function toggleActive(
    user: ApplicationUser,
  ) {
    setSavingUserId(user.id);
    setError(null);
    setSuccess(null);

    try {
      await authApi.updateUser(
        user.id,
        {
          firstName: user.firstName,
          lastName: user.lastName,
          role: user.role,
          isActive: !user.isActive,
        },
      );

      setSuccess(
        `${user.email} is now ${
          user.isActive
            ? "inactive"
            : "active"
        }.`,
      );

      await load();
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : "Unable to update user.",
      );
    } finally {
      setSavingUserId(null);
    }
  }

  return (
    <section className="panel">
      <div className="panel-heading">
        <div>
          <p className="eyebrow">
            Administration
          </p>
          <h2>Users &amp; Roles</h2>
          <p className="muted">
            Manage application access,
            account details, and role
            assignments.
          </p>
        </div>
      </div>

      {error && (
        <div className="alert error">
          {error}
        </div>
      )}

      {success && (
        <div className="alert success">
          {success}
        </div>
      )}

      <form
        className="v09-form-card"
        onSubmit={create}
      >
        <h3>Create user</h3>

        <div className="v09-form-grid">
          <label className="field">
            <span>First name *</span>
            <input
              value={firstName}
              onChange={(event) =>
                setFirstName(
                  event.target.value,
                )
              }
            />
          </label>

          <label className="field">
            <span>Last name *</span>
            <input
              value={lastName}
              onChange={(event) =>
                setLastName(
                  event.target.value,
                )
              }
            />
          </label>

          <label className="field">
            <span>Email *</span>
            <input
              type="email"
              value={email}
              onChange={(event) =>
                setEmail(
                  event.target.value,
                )
              }
            />
          </label>

          <label className="field">
            <span>
              Temporary password *
            </span>
            <input
              type="password"
              minLength={12}
              value={password}
              onChange={(event) =>
                setPassword(
                  event.target.value,
                )
              }
            />
          </label>

          <label className="field">
            <span>Role *</span>
            <select
              value={role}
              onChange={(event) =>
                setRole(
                  event.target
                    .value as AimsRole,
                )
              }
            >
              {ROLES.map((item) => (
                <option
                  key={item}
                  value={item}
                >
                  {item}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div className="filter-actions">
          <button
            type="submit"
            className="button primary"
          >
            Create user
          </button>
        </div>
      </form>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>User</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Last Login</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            {users.map((user) => {
              const isEditing =
                editing?.userId ===
                user.id;

              const isCurrentAccount =
                user.id === currentUserId;

              return (
                <tr key={user.id}>
                  <td>
                    {isEditing ? (
                      <div className="user-edit-name-grid">
                        <input
                          aria-label="First name"
                          value={
                            editing.firstName
                          }
                          onChange={(
                            event,
                          ) =>
                            setEditing({
                              ...editing,
                              firstName:
                                event
                                  .target
                                  .value,
                            })
                          }
                        />

                        <input
                          aria-label="Last name"
                          value={
                            editing.lastName
                          }
                          onChange={(
                            event,
                          ) =>
                            setEditing({
                              ...editing,
                              lastName:
                                event
                                  .target
                                  .value,
                            })
                          }
                        />
                      </div>
                    ) : (
                      <>
                        <strong>
                          {
                            user.displayName
                          }
                        </strong>

                        {isCurrentAccount && (
                          <span className="cell-subtitle">
                            Current account
                          </span>
                        )}
                      </>
                    )}
                  </td>

                  <td>{user.email}</td>

                  <td>
                    {isEditing ? (
                      <select
                        className="user-role-select"
                        value={
                          editing.role
                        }
                        onChange={(
                          event,
                        ) =>
                          setEditing({
                            ...editing,
                            role: event
                              .target
                              .value as AimsRole,
                          })
                        }
                        disabled={
                          isCurrentAccount
                        }
                        title={
                          isCurrentAccount
                            ? "Your own role cannot be changed from the current session."
                            : undefined
                        }
                      >
                        {ROLES.map(
                          (item) => (
                            <option
                              key={
                                item
                              }
                              value={
                                item
                              }
                            >
                              {item}
                            </option>
                          ),
                        )}
                      </select>
                    ) : (
                      user.role
                    )}
                  </td>

                  <td>
                    <span className="badge">
                      {user.isActive
                        ? "Active"
                        : "Inactive"}
                    </span>
                  </td>

                  <td>
                    {user.lastLoginAt
                      ? new Date(
                          user.lastLoginAt,
                        ).toLocaleString()
                      : "—"}
                  </td>

                  <td>
                    <div className="user-row-actions">
                      {isEditing ? (
                        <>
                          <button
                            type="button"
                            className="button primary compact"
                            disabled={
                              savingUserId ===
                              user.id
                            }
                            onClick={() =>
                              saveEdit(
                                user,
                              )
                            }
                          >
                            {savingUserId ===
                            user.id
                              ? "Saving…"
                              : "Save"}
                          </button>

                          <button
                            type="button"
                            className="button secondary compact"
                            onClick={
                              cancelEdit
                            }
                          >
                            Cancel
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            type="button"
                            className="button secondary compact"
                            onClick={() =>
                              beginEdit(
                                user,
                              )
                            }
                          >
                            Edit
                          </button>

                          <button
                            type="button"
                            className="button secondary compact"
                            disabled={
                              isCurrentAccount ||
                              savingUserId ===
                                user.id
                            }
                            title={
                              isCurrentAccount
                                ? "You cannot deactivate the account you are currently using."
                                : undefined
                            }
                            onClick={() =>
                              toggleActive(
                                user,
                              )
                            }
                          >
                            {user.isActive
                              ? "Deactivate"
                              : "Activate"}
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      <p className="muted user-role-note">
        Role changes take effect on the
        user's next sign-in because role
        permissions are included in the
        authentication token issued at
        login.
      </p>
    </section>
  );
}
