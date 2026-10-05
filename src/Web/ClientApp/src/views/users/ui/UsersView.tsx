"use client";

import {
  ActionIcon,
  Badge,
  Button,
  Group,
  Modal,
  MultiSelect,
  PasswordInput,
  Stack,
  Switch,
  Table,
  Text,
  TextInput,
  Title,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useUnit } from "effector-react";
import { KeyRound, Pencil, Plus, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
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
import type { IdentityUserDto } from "@/web-api-client";

export function UsersView() {
  const [users, roles, loading, myPermissions] = useUnit([$users, $roles, $usersLoading, $permissions]);

  // Mirrors the API's rules (the API enforces them regardless): editing users needs users.write,
  // and assigning roles additionally needs roles.write — plus roles.read to list the options.
  const canWriteUsers = myPermissions.includes(PERMISSIONS.users.write);
  const canReadRoles = myPermissions.includes(PERMISSIONS.roles.read);
  const canAssignRoles = canWriteUsers && canReadRoles && myPermissions.includes(PERMISSIONS.roles.write);
  const roleOptions = roles.flatMap((r) => (r.name ? [r.name] : []));

  useEffect(() => {
    fetchUsersFx();
  }, []);

  useEffect(() => {
    if (canReadRoles) fetchRolesFx();
  }, [canReadRoles]);

  // ── New user dialog ────────────────────────────────────────────────────
  const [newUserOpened, { open: openNewUser, close: closeNewUser }] = useDisclosure(false);
  const [newUsername, setNewUsername] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [newFirstName, setNewFirstName] = useState("");
  const [newLastName, setNewLastName] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [newRoles, setNewRoles] = useState<string[]>([]);
  const [newUserError, setNewUserError] = useState("");

  const showNewUserDialog = () => {
    setNewUsername("");
    setNewEmail("");
    setNewFirstName("");
    setNewLastName("");
    setNewPassword("");
    setNewRoles([]);
    setNewUserError("");
    openNewUser();
  };

  const commitNewUser = async () => {
    if (!newUsername.trim()) return;
    try {
      await createUserFx({
        username: newUsername.trim(),
        email: newEmail.trim() || undefined,
        firstName: newFirstName.trim() || undefined,
        lastName: newLastName.trim() || undefined,
        password: newPassword.trim() || undefined,
        temporaryPassword: true,
        roles: canAssignRoles ? newRoles : [],
      });
      closeNewUser();
    } catch (e) {
      setNewUserError(getFieldError(e, "Username") ?? "Failed to create user.");
    }
  };

  // ── Edit user dialog ───────────────────────────────────────────────────
  const [editUserOpened, { open: openEditUser, close: closeEditUser }] = useDisclosure(false);
  const [editingUser, setEditingUser] = useState<IdentityUserDto | null>(null);
  const [editUsername, setEditUsername] = useState("");
  const [editEmail, setEditEmail] = useState("");
  const [editFirstName, setEditFirstName] = useState("");
  const [editLastName, setEditLastName] = useState("");
  const [editEnabled, setEditEnabled] = useState(true);
  const [editRoles, setEditRoles] = useState<string[]>([]);
  const [editUserError, setEditUserError] = useState("");

  const showEditUserDialog = (user: IdentityUserDto) => {
    setEditingUser(user);
    setEditUsername(user.username ?? "");
    setEditEmail(user.email ?? "");
    setEditFirstName(user.firstName ?? "");
    setEditLastName(user.lastName ?? "");
    setEditEnabled(user.enabled ?? true);
    setEditRoles(user.roles ?? []);
    setEditUserError("");
    openEditUser();
  };

  const commitEditUser = async () => {
    if (!editingUser?.id || !editUsername.trim()) return;
    try {
      await updateUserFx({
        id: editingUser.id,
        username: editUsername.trim(),
        email: editEmail.trim() || undefined,
        firstName: editFirstName.trim() || undefined,
        lastName: editLastName.trim() || undefined,
        enabled: editEnabled,
      });
      if (canAssignRoles) {
        await setUserRolesFx({ id: editingUser.id, roles: editRoles });
      }
      closeEditUser();
    } catch (e) {
      setEditUserError(getFieldError(e, "Username") ?? "Failed to update user.");
    }
  };

  // ── Reset password dialog ──────────────────────────────────────────────
  const [resetPasswordOpened, { open: openResetPassword, close: closeResetPassword }] = useDisclosure(false);
  const [resetPasswordUser, setResetPasswordUser] = useState<IdentityUserDto | null>(null);
  const [resetPasswordValue, setResetPasswordValue] = useState("");
  const [resetPasswordTemporary, setResetPasswordTemporary] = useState(true);
  const [resetPasswordError, setResetPasswordError] = useState("");

  const showResetPasswordDialog = (user: IdentityUserDto) => {
    setResetPasswordUser(user);
    setResetPasswordValue("");
    setResetPasswordTemporary(true);
    setResetPasswordError("");
    openResetPassword();
  };

  const commitResetPassword = async () => {
    if (!resetPasswordUser?.id || !resetPasswordValue) return;
    try {
      await resetPasswordFx({
        id: resetPasswordUser.id,
        password: resetPasswordValue,
        temporary: resetPasswordTemporary,
      });
      closeResetPassword();
    } catch (e) {
      setResetPasswordError(getFieldError(e, "Password") ?? "Failed to reset password.");
    }
  };

  // ── Delete user confirmation ───────────────────────────────────────────
  const [deleteUserOpened, { open: openDeleteUser, close: closeDeleteUser }] = useDisclosure(false);
  const [deletingUser, setDeletingUser] = useState<IdentityUserDto | null>(null);

  const confirmDeleteUser = (user: IdentityUserDto) => {
    setDeletingUser(user);
    openDeleteUser();
  };

  const deleteUserConfirmed = async () => {
    if (!deletingUser?.id) return;
    await deleteUserFx(deletingUser.id);
    closeDeleteUser();
  };

  return (
    <div>
      <Group justify="space-between" mb="md">
        <div>
          <Title order={1}>Users</Title>
          <Text>Manage Keycloak users and the roles assigned to them.</Text>
        </div>
        {canWriteUsers && (
          <Button leftSection={<Plus size={16} />} onClick={showNewUserDialog}>
            New user
          </Button>
        )}
      </Group>

      {loading && users.length === 0 ? (
        <Text aria-busy="true">Loading&hellip;</Text>
      ) : (
        <Table>
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
            {users.map((user) => (
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
                    <Group gap={4} wrap="nowrap">
                      <ActionIcon variant="subtle" aria-label="Edit user" onClick={() => showEditUserDialog(user)}>
                        <Pencil size={18} strokeWidth={2} />
                      </ActionIcon>
                      <ActionIcon
                        variant="subtle"
                        aria-label="Reset password"
                        onClick={() => showResetPasswordDialog(user)}
                      >
                        <KeyRound size={18} strokeWidth={2} />
                      </ActionIcon>
                      <ActionIcon
                        variant="subtle"
                        color="red"
                        aria-label="Delete user"
                        onClick={() => confirmDeleteUser(user)}
                      >
                        <Trash2 size={18} strokeWidth={2} />
                      </ActionIcon>
                    </Group>
                  </Table.Td>
                )}
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {/* New user dialog */}
      <Modal opened={newUserOpened} onClose={closeNewUser} title="New User">
        <Stack>
          <TextInput
            label="Username"
            value={newUsername}
            onChange={(e) => setNewUsername(e.currentTarget.value)}
            error={newUserError}
            autoFocus
          />
          <TextInput label="Email" type="email" value={newEmail} onChange={(e) => setNewEmail(e.currentTarget.value)} />
          <Group grow>
            <TextInput
              label="First name"
              value={newFirstName}
              onChange={(e) => setNewFirstName(e.currentTarget.value)}
            />
            <TextInput label="Last name" value={newLastName} onChange={(e) => setNewLastName(e.currentTarget.value)} />
          </Group>
          <PasswordInput
            label="Initial password"
            description="Leave blank to require the user to set one via a forgot-password flow."
            value={newPassword}
            onChange={(e) => setNewPassword(e.currentTarget.value)}
          />
          {canAssignRoles && (
            <MultiSelect label="Roles" data={roleOptions} value={newRoles} onChange={setNewRoles} />
          )}
          <Group justify="flex-end">
            <Button variant="default" onClick={closeNewUser}>
              Cancel
            </Button>
            <Button onClick={commitNewUser}>Create</Button>
          </Group>
        </Stack>
      </Modal>

      {/* Edit user dialog */}
      <Modal opened={editUserOpened} onClose={closeEditUser} title="Edit User">
        <Stack>
          <TextInput
            label="Username"
            value={editUsername}
            onChange={(e) => setEditUsername(e.currentTarget.value)}
            error={editUserError}
          />
          <TextInput
            label="Email"
            type="email"
            value={editEmail}
            onChange={(e) => setEditEmail(e.currentTarget.value)}
          />
          <Group grow>
            <TextInput
              label="First name"
              value={editFirstName}
              onChange={(e) => setEditFirstName(e.currentTarget.value)}
            />
            <TextInput
              label="Last name"
              value={editLastName}
              onChange={(e) => setEditLastName(e.currentTarget.value)}
            />
          </Group>
          <Switch
            label="Enabled"
            checked={editEnabled}
            onChange={(e) => setEditEnabled(e.currentTarget.checked)}
          />
          {canAssignRoles && (
            <MultiSelect label="Roles" data={roleOptions} value={editRoles} onChange={setEditRoles} />
          )}
          <Group justify="flex-end">
            <Button variant="default" onClick={closeEditUser}>
              Cancel
            </Button>
            <Button onClick={commitEditUser}>Update</Button>
          </Group>
        </Stack>
      </Modal>

      {/* Reset password dialog */}
      <Modal opened={resetPasswordOpened} onClose={closeResetPassword} title="Reset Password">
        <Stack>
          <Text size="sm" c="dimmed">
            Set a new password for <strong>{resetPasswordUser?.username}</strong>.
          </Text>
          <PasswordInput
            label="New password"
            value={resetPasswordValue}
            onChange={(e) => setResetPasswordValue(e.currentTarget.value)}
            error={resetPasswordError}
            autoFocus
          />
          <Switch
            label="Require password change on next login"
            checked={resetPasswordTemporary}
            onChange={(e) => setResetPasswordTemporary(e.currentTarget.checked)}
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={closeResetPassword}>
              Cancel
            </Button>
            <Button onClick={commitResetPassword}>Reset</Button>
          </Group>
        </Stack>
      </Modal>

      {/* Delete user confirmation */}
      <Modal opened={deleteUserOpened} onClose={closeDeleteUser} title={`Delete "${deletingUser?.username}"?`}>
        <Stack>
          <Text>This permanently deletes the user from Keycloak.</Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeDeleteUser}>
              Cancel
            </Button>
            <Button color="red" onClick={deleteUserConfirmed}>
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>
    </div>
  );
}
