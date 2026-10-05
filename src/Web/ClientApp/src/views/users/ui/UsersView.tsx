"use client";

import {
  ActionIcon,
  Alert,
  Badge,
  Button,
  Group,
  Modal,
  MultiSelect,
  Pagination,
  Paper,
  PasswordInput,
  Skeleton,
  Stack,
  Switch,
  Table,
  Text,
  TextInput,
  Tooltip,
} from "@mantine/core";
import { useDebouncedValue } from "@mantine/hooks";
import { useUnit } from "effector-react";
import { Info, KeyRound, Pencil, Plus, Search, SearchX, Trash2, UserX } from "lucide-react";
import { useSearchParams } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { $roles, fetchRolesFx } from "@/entities/role";
import { $permissions } from "@/entities/session";
import {
  $users,
  $usersLoading,
  createUserFx,
  deleteUserFx,
  fetchUsersFx,
  resetPasswordFx,
  setUserRolesFx,
  updateUserFx,
} from "@/entities/user";
import { PERMISSIONS } from "@/shared/config/permissions";
import { getFieldError } from "@/shared/lib/api-error";
import { notifyError, notifySuccess } from "@/shared/lib/notify";
import { EmptyState } from "@/shared/ui/EmptyState";
import { PageHeader } from "@/shared/ui/PageHeader";
import type { IdentityRoleDto, IdentityUserDto } from "@/web-api-client";

const PAGE_SIZE = 10;

type Dialog =
  | { kind: "create" }
  | { kind: "edit"; user: IdentityUserDto }
  | { kind: "password"; user: IdentityUserDto }
  | { kind: "delete"; user: IdentityUserDto }
  | null;

/** Reads an initial search from ?search= (the Ctrl+K search links here); keyed so a new link resets the page. */
export function UsersView() {
  const initialSearch = useSearchParams().get("search") ?? "";
  return <UsersScreen key={initialSearch} initialSearch={initialSearch} />;
}

function UsersScreen({ initialSearch }: { initialSearch: string }) {
  const [users, roles, loading, myPermissions] = useUnit([$users, $roles, $usersLoading, $permissions]);
  const [dialog, setDialog] = useState<Dialog>(null);
  const [search, setSearch] = useState(initialSearch);
  const [debouncedSearch] = useDebouncedValue(search.trim(), 300);
  const [page, setPage] = useState(1);
  const changeSearch = (value: string) => {
    setSearch(value);
    setPage(1);
  };

  // Mirrors the API's rules (the API enforces them regardless): editing users needs users.write,
  // and assigning roles additionally needs roles.write — plus roles.read to list the options.
  const canWriteUsers = myPermissions.includes(PERMISSIONS.users.write);
  const canReadRoles = myPermissions.includes(PERMISSIONS.roles.read);
  const canAssignRoles = canWriteUsers && canReadRoles && myPermissions.includes(PERMISSIONS.roles.write);

  // Keycloak does the matching (username, email, first and last name); paging is client-side.
  useEffect(() => {
    fetchUsersFx(debouncedSearch || undefined).catch((e) => notifyError("Couldn't load users", e));
  }, [debouncedSearch]);

  const pageCount = Math.max(1, Math.ceil(users.length / PAGE_SIZE));
  const currentPage = Math.min(page, pageCount);
  const pageUsers = useMemo(
    () => users.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE),
    [users, currentPage],
  );

  useEffect(() => {
    if (canReadRoles) fetchRolesFx().catch(() => undefined);
  }, [canReadRoles]);

  const close = () => setDialog(null);

  return (
    <>
      <PageHeader
        title="Users"
        description="Manage people and the roles assigned to them."
        breadcrumbs={[{ label: "Admin", href: "/admin" }, { label: "Users" }]}
        action={
          canWriteUsers && (
            <Button leftSection={<Plus size={16} />} onClick={() => setDialog({ kind: "create" })}>
              New user
            </Button>
          )
        }
      />

      <Group justify="space-between" mb="md" gap="sm">
        <TextInput
          placeholder="Search by username, name or email"
          aria-label="Search users"
          leftSection={<Search size={16} />}
          value={search}
          onChange={(e) => changeSearch(e.currentTarget.value)}
          w={{ base: "100%", xs: 340 }}
        />
        {users.length > 0 && (
          <Text c="dimmed" fz="sm" style={{ fontVariantNumeric: "tabular-nums" }}>
            {users.length === 1 ? "1 user" : `${users.length} users`}
            {pageCount > 1 && ` · showing ${(currentPage - 1) * PAGE_SIZE + 1}–${(currentPage - 1) * PAGE_SIZE + pageUsers.length}`}
          </Text>
        )}
      </Group>

      {loading && users.length === 0 ? (
        <Stack gap="xs">
          {Array.from({ length: 4 }, (_, i) => (
            <Skeleton key={i} h={40} radius="sm" />
          ))}
        </Stack>
      ) : users.length === 0 && debouncedSearch ? (
        <EmptyState
          icon={<SearchX size={28} />}
          title={`No users match "${debouncedSearch}"`}
          action={
            <Button variant="light" size="xs" onClick={() => changeSearch("")}>
              Clear search
            </Button>
          }
        >
          Search matches usernames, email addresses, and first and last names.
        </EmptyState>
      ) : users.length === 0 ? (
        <EmptyState icon={<UserX size={28} />} title="No users yet">
          Users you create here can sign in through Keycloak.
        </EmptyState>
      ) : (
        <Table.ScrollContainer minWidth={760}>
          <Table verticalSpacing="sm" highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Username</Table.Th>
                <Table.Th>Name</Table.Th>
                <Table.Th>Email</Table.Th>
                <Table.Th>Roles</Table.Th>
                <Table.Th>Status</Table.Th>
                {canWriteUsers && <Table.Th />}
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {pageUsers.map((user) => (
                <Table.Tr key={user.id}>
                  <Table.Td>{user.username}</Table.Td>
                  <Table.Td>{[user.firstName, user.lastName].filter(Boolean).join(" ")}</Table.Td>
                  <Table.Td>{user.email}</Table.Td>
                  <Table.Td>
                    <Group gap={4}>
                      {(user.roles ?? []).map((role) => (
                        <Badge key={role} variant="light">
                          {role}
                        </Badge>
                      ))}
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={user.enabled ? "green" : "gray"} variant="light">
                      {user.enabled ? "Enabled" : "Disabled"}
                    </Badge>
                  </Table.Td>
                  {canWriteUsers && (
                    <Table.Td>
                      <Group gap={2} justify="flex-end" wrap="nowrap">
                        <Tooltip label="Edit">
                          <ActionIcon variant="subtle" aria-label={`Edit ${user.username}`} onClick={() => setDialog({ kind: "edit", user })}>
                            <Pencil size={18} />
                          </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Reset password">
                          <ActionIcon
                            variant="subtle"
                            aria-label={`Reset password for ${user.username}`}
                            onClick={() => setDialog({ kind: "password", user })}
                          >
                            <KeyRound size={18} />
                          </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Delete">
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            aria-label={`Delete ${user.username}`}
                            onClick={() => setDialog({ kind: "delete", user })}
                          >
                            <Trash2 size={18} />
                          </ActionIcon>
                        </Tooltip>
                      </Group>
                    </Table.Td>
                  )}
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {pageCount > 1 && (
        <Group justify="flex-end" mt="md">
          <Pagination total={pageCount} value={currentPage} onChange={setPage} size="sm" />
        </Group>
      )}

      {(dialog?.kind === "create" || dialog?.kind === "edit") && (
        <UserFormModal
          user={dialog.kind === "edit" ? dialog.user : undefined}
          allRoles={roles}
          canAssignRoles={canAssignRoles}
          onClose={close}
        />
      )}
      {dialog?.kind === "password" && <ResetPasswordModal user={dialog.user} onClose={close} />}
      {dialog?.kind === "delete" && <DeleteUserModal user={dialog.user} onClose={close} />}
    </>
  );
}

interface UserFormModalProps {
  /** Omitted when creating a new user. */
  user?: IdentityUserDto;
  allRoles: IdentityRoleDto[];
  canAssignRoles: boolean;
  onClose: () => void;
}

function UserFormModal({ user, allRoles, canAssignRoles, onClose }: UserFormModalProps) {
  const isEdit = !!user;
  const [username, setUsername] = useState(user?.username ?? "");
  const [email, setEmail] = useState(user?.email ?? "");
  const [firstName, setFirstName] = useState(user?.firstName ?? "");
  const [lastName, setLastName] = useState(user?.lastName ?? "");
  const [password, setPassword] = useState("");
  const [enabled, setEnabled] = useState(user?.enabled ?? true);
  const [roles, setRoles] = useState<string[]>(user?.roles ?? []);
  const [errors, setErrors] = useState<Record<string, string | undefined>>({});
  const saving = useUnit([createUserFx.pending, updateUserFx.pending, setUserRolesFx.pending]).some(Boolean);

  const submit = async () => {
    if (!username.trim()) {
      setErrors({ username: "Enter a username." });
      return;
    }
    const profile = {
      username: username.trim(),
      email: email.trim() || undefined,
      firstName: firstName.trim() || undefined,
      lastName: lastName.trim() || undefined,
    };
    try {
      if (user?.id) {
        await updateUserFx({ id: user.id, ...profile, enabled });
        if (canAssignRoles) await setUserRolesFx({ id: user.id, roles });
        notifySuccess("User updated", `Changes apply to ${profile.username} at their next sign-in.`);
      } else {
        await createUserFx({ ...profile, password: password || undefined, temporaryPassword: true, roles: canAssignRoles ? roles : [] });
        notifySuccess("User created", `${profile.username} can now sign in.`);
      }
      onClose();
    } catch (e) {
      const fieldErrors = {
        username: getFieldError(e, "Username"),
        email: getFieldError(e, "Email"),
        password: getFieldError(e, "Password"),
      };
      if (Object.values(fieldErrors).some(Boolean)) setErrors(fieldErrors);
      else notifyError(isEdit ? "Couldn't update the user" : "Couldn't create the user", e);
    }
  };

  return (
    <Modal opened onClose={saving ? () => {} : onClose} title={isEdit ? "Edit user" : "New user"}>
      <Stack>
        <TextInput label="Username" value={username} onChange={(e) => setUsername(e.currentTarget.value)} error={errors.username} data-autofocus />
        <TextInput label="Email" type="email" value={email} onChange={(e) => setEmail(e.currentTarget.value)} error={errors.email} />
        <Group grow>
          <TextInput label="First name" value={firstName} onChange={(e) => setFirstName(e.currentTarget.value)} />
          <TextInput label="Last name" value={lastName} onChange={(e) => setLastName(e.currentTarget.value)} />
        </Group>
        {isEdit ? (
          <Switch label="Enabled" checked={enabled} onChange={(e) => setEnabled(e.currentTarget.checked)} />
        ) : (
          <PasswordInput
            label="Initial password"
            description="Leave blank to have the user set one through the forgot-password flow. They must change it at first sign-in."
            value={password}
            onChange={(e) => setPassword(e.currentTarget.value)}
            error={errors.password}
          />
        )}
        {canAssignRoles && (
          <>
            <MultiSelect
              label="Roles"
              data={allRoles.flatMap((r) => (r.name ? [r.name] : []))}
              value={roles}
              onChange={setRoles}
              searchable
            />
            <EffectivePermissions roles={roles} allRoles={allRoles} />
            {isEdit && (
              <Alert variant="light" color="blue" icon={<Info size={16} />} p="xs">
                Role changes take effect at the user&apos;s next sign-in.
              </Alert>
            )}
          </>
        )}
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose} disabled={saving}>
            Cancel
          </Button>
          <Button onClick={submit} loading={saving}>
            {isEdit ? "Update" : "Create"}
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

/** Read-only union of the permissions the selected roles grant — what the user will be able to do. */
function EffectivePermissions({ roles, allRoles }: { roles: string[]; allRoles: IdentityRoleDto[] }) {
  const permissions = [
    ...new Set(allRoles.filter((r) => r.name && roles.includes(r.name)).flatMap((r) => r.permissions ?? [])),
  ].sort();

  return (
    <Paper withBorder radius="sm" p="sm" bg="var(--mantine-color-default-hover)">
      <Text fz="xs" fw={600} tt="uppercase" c="dimmed" mb={6} style={{ letterSpacing: "0.04em" }}>
        Effective permissions
      </Text>
      {permissions.length === 0 ? (
        <Text fz="sm" c="dimmed">
          No permissions — the user can sign in but can&apos;t see or change anything.
        </Text>
      ) : (
        <Group gap={4}>
          {permissions.map((permission) => (
            <Badge key={permission} variant="light" color="gray" tt="none">
              {permission}
            </Badge>
          ))}
        </Group>
      )}
    </Paper>
  );
}

function ResetPasswordModal({ user, onClose }: { user: IdentityUserDto; onClose: () => void }) {
  const [password, setPassword] = useState("");
  const [temporary, setTemporary] = useState(true);
  const [error, setError] = useState<string>();
  const saving = useUnit(resetPasswordFx.pending);

  const submit = async () => {
    if (!user.id || !password) {
      setError("Enter a new password.");
      return;
    }
    try {
      await resetPasswordFx({ id: user.id, password, temporary });
      notifySuccess(
        "Password reset",
        temporary ? `${user.username} must choose a new password at next sign-in.` : `${user.username} can sign in with the new password.`,
      );
      onClose();
    } catch (e) {
      const fieldError = getFieldError(e, "Password");
      if (fieldError) setError(fieldError);
      else notifyError("Couldn't reset the password", e);
    }
  };

  return (
    <Modal opened onClose={saving ? () => {} : onClose} title="Reset password">
      <Stack>
        <Text size="sm" c="dimmed">
          Set a new password for <strong>{user.username}</strong>.
        </Text>
        <PasswordInput label="New password" value={password} onChange={(e) => setPassword(e.currentTarget.value)} error={error} data-autofocus />
        <Switch label="Require a new password at next sign-in" checked={temporary} onChange={(e) => setTemporary(e.currentTarget.checked)} />
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose} disabled={saving}>
            Cancel
          </Button>
          <Button onClick={submit} loading={saving}>
            Reset password
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}

function DeleteUserModal({ user, onClose }: { user: IdentityUserDto; onClose: () => void }) {
  const saving = useUnit(deleteUserFx.pending);

  const submit = async () => {
    if (!user.id) return;
    try {
      await deleteUserFx(user.id);
      notifySuccess("User deleted", `${user.username} was removed from Keycloak.`);
      onClose();
    } catch (e) {
      notifyError("Couldn't delete the user", e);
    }
  };

  return (
    <Modal opened onClose={saving ? () => {} : onClose} title={`Delete "${user.username}"?`}>
      <Stack>
        <Text>This permanently deletes the user from Keycloak. They won&apos;t be able to sign in again.</Text>
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose} disabled={saving}>
            Cancel
          </Button>
          <Button color="red" onClick={submit} loading={saving}>
            Delete
          </Button>
        </Group>
      </Stack>
    </Modal>
  );
}
