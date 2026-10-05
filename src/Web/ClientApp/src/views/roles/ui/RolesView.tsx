"use client";

import {
  ActionIcon,
  Badge,
  Button,
  Checkbox,
  Group,
  Modal,
  SimpleGrid,
  Stack,
  Table,
  Text,
  TextInput,
  Title,
  Tooltip,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useUnit } from "effector-react";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { useEffect, useState } from "react";
import {
  $permissionCatalog,
  $roles,
  $rolesLoading,
  createRoleFx,
  deleteRoleFx,
  fetchPermissionsFx,
  fetchRolesFx,
  updateRoleFx,
} from "@/entities/role";
import { $permissions } from "@/entities/session";
import { getFieldError } from "@/shared/lib/api-error";
import { ADMINISTRATOR_ROLE, PERMISSIONS } from "@/shared/config/permissions";
import type { IdentityPermissionDto, IdentityRoleDto } from "@/web-api-client";

// "todolists.read" → "todolists"; the catalog is grouped by resource in the editor.
const resourceOf = (permission: string) => permission.split(".")[0];

function groupByResource(catalog: IdentityPermissionDto[]) {
  const groups = new Map<string, IdentityPermissionDto[]>();
  for (const permission of catalog) {
    const resource = resourceOf(permission.name ?? "");
    groups.set(resource, [...(groups.get(resource) ?? []), permission]);
  }
  return [...groups.entries()];
}

export function RolesView() {
  const [roles, catalog, loading, myPermissions] = useUnit([$roles, $permissionCatalog, $rolesLoading, $permissions]);
  const canWrite = myPermissions.includes(PERMISSIONS.roles.write);

  useEffect(() => {
    fetchRolesFx();
    fetchPermissionsFx();
  }, []);

  // ── Create / edit dialog ───────────────────────────────────────────────
  const [editorOpened, { open: openEditor, close: closeEditor }] = useDisclosure(false);
  const [editingRole, setEditingRole] = useState<IdentityRoleDto | null>(null);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [selected, setSelected] = useState<string[]>([]);
  const [error, setError] = useState("");

  const showEditor = (role: IdentityRoleDto | null) => {
    setEditingRole(role);
    setName(role?.name ?? "");
    setDescription(role?.description ?? "");
    setSelected(role?.permissions ?? []);
    setError("");
    openEditor();
  };

  const commitEditor = async () => {
    if (!name.trim()) return;
    const params = { name: name.trim(), description: description.trim() || undefined, permissions: selected };
    try {
      if (editingRole) {
        await updateRoleFx(params);
      } else {
        await createRoleFx(params);
      }
      closeEditor();
    } catch (e) {
      setError(getFieldError(e, "Name") ?? `Failed to ${editingRole ? "update" : "create"} role.`);
    }
  };

  // ── Delete confirmation ────────────────────────────────────────────────
  const [deleteOpened, { open: openDelete, close: closeDelete }] = useDisclosure(false);
  const [deletingRole, setDeletingRole] = useState<IdentityRoleDto | null>(null);

  const confirmDelete = (role: IdentityRoleDto) => {
    setDeletingRole(role);
    openDelete();
  };

  const deleteConfirmed = async () => {
    if (!deletingRole?.name) return;
    await deleteRoleFx(deletingRole.name);
    closeDelete();
  };

  return (
    <div>
      <Group justify="space-between" mb="md">
        <div>
          <Title order={1}>Roles</Title>
          <Text>Roles bundle permissions. Users are assigned roles, never permissions directly.</Text>
        </div>
        {canWrite && (
          <Button leftSection={<Plus size={16} />} onClick={() => showEditor(null)}>
            New role
          </Button>
        )}
      </Group>

      {loading && roles.length === 0 ? (
        <Text aria-busy="true">Loading&hellip;</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>Name</Table.Th>
              <Table.Th>Description</Table.Th>
              <Table.Th>Permissions</Table.Th>
              {canWrite && <Table.Th />}
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {roles.map((role) => {
              const isBuiltIn = role.name === ADMINISTRATOR_ROLE;
              return (
                <Table.Tr key={role.name}>
                  <Table.Td>
                    <Group gap="xs" wrap="nowrap">
                      <Text fw={500}>{role.name}</Text>
                      {isBuiltIn && (
                        <Badge size="xs" variant="outline" color="gray">
                          Built-in
                        </Badge>
                      )}
                    </Group>
                  </Table.Td>
                  <Table.Td>{role.description}</Table.Td>
                  <Table.Td>
                    <Group gap={4}>
                      {(role.permissions ?? []).map((permission) => (
                        <Badge key={permission} variant="light">
                          {permission}
                        </Badge>
                      ))}
                    </Group>
                  </Table.Td>
                  {canWrite && (
                    <Table.Td>
                      {isBuiltIn ? (
                        <Tooltip label="The Administrator role can't be modified">
                          <Text size="xs" c="dimmed">
                            Locked
                          </Text>
                        </Tooltip>
                      ) : (
                        <Group gap={4} wrap="nowrap">
                          <ActionIcon variant="subtle" aria-label="Edit role" onClick={() => showEditor(role)}>
                            <Pencil size={18} strokeWidth={2} />
                          </ActionIcon>
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            aria-label="Delete role"
                            onClick={() => confirmDelete(role)}
                          >
                            <Trash2 size={18} strokeWidth={2} />
                          </ActionIcon>
                        </Group>
                      )}
                    </Table.Td>
                  )}
                </Table.Tr>
              );
            })}
          </Table.Tbody>
        </Table>
      )}

      {/* Create / edit dialog */}
      <Modal opened={editorOpened} onClose={closeEditor} title={editingRole ? `Edit "${editingRole.name}"` : "New Role"} size="lg">
        <Stack>
          <TextInput
            label="Name"
            description={editingRole ? "Role names can't be changed." : undefined}
            value={name}
            onChange={(e) => setName(e.currentTarget.value)}
            disabled={!!editingRole}
            error={error}
            autoFocus={!editingRole}
          />
          <TextInput label="Description" value={description} onChange={(e) => setDescription(e.currentTarget.value)} />
          <Checkbox.Group label="Permissions" value={selected} onChange={setSelected}>
            <Stack gap="sm" mt="xs">
              {groupByResource(catalog).map(([resource, permissions]) => (
                <div key={resource}>
                  <Text size="sm" fw={600} tt="capitalize" mb={4}>
                    {resource}
                  </Text>
                  <SimpleGrid cols={2} spacing="xs">
                    {permissions.map((permission) => (
                      <Checkbox
                        key={permission.name}
                        value={permission.name}
                        label={permission.name}
                        description={permission.description}
                      />
                    ))}
                  </SimpleGrid>
                </div>
              ))}
            </Stack>
          </Checkbox.Group>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeEditor}>
              Cancel
            </Button>
            <Button onClick={commitEditor}>{editingRole ? "Update" : "Create"}</Button>
          </Group>
        </Stack>
      </Modal>

      {/* Delete confirmation */}
      <Modal opened={deleteOpened} onClose={closeDelete} title={`Delete "${deletingRole?.name}"?`}>
        <Stack>
          <Text>The role is removed from every user who holds it, and they lose its permissions.</Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={closeDelete}>
              Cancel
            </Button>
            <Button color="red" onClick={deleteConfirmed}>
              Delete
            </Button>
          </Group>
        </Stack>
      </Modal>
    </div>
  );
}
