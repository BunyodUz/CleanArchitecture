"use client";

import {
  ActionIcon,
  Alert,
  Badge,
  Button,
  Checkbox,
  Group,
  Modal,
  SimpleGrid,
  Skeleton,
  Stack,
  Table,
  Text,
  TextInput,
  Tooltip,
} from "@mantine/core";
import { useUnit } from "effector-react";
import { Info, Lock, Pencil, Plus, ShieldOff, Trash2 } from "lucide-react";
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
import { PERMISSIONS } from "@/shared/config/permissions";
import { getFieldError } from "@/shared/lib/api-error";
import { notifyError, notifySuccess } from "@/shared/lib/notify";
import { EmptyState } from "@/shared/ui/EmptyState";
import { PageHeader } from "@/shared/ui/PageHeader";
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

type Dialog = { kind: "create" } | { kind: "edit"; role: IdentityRoleDto } | { kind: "delete"; role: IdentityRoleDto } | null;

export function RolesView() {
  const [roles, catalog, loading, myPermissions] = useUnit([$roles, $permissionCatalog, $rolesLoading, $permissions]);
  const canWrite = myPermissions.includes(PERMISSIONS.roles.write);
  const [dialog, setDialog] = useState<Dialog>(null);

  useEffect(() => {
    fetchRolesFx().catch((e) => notifyError("Couldn't load roles", e));
    fetchPermissionsFx().catch(() => undefined);
  }, []);

  const close = () => setDialog(null);

  return (
    <>
      <PageHeader
        title="Roles"
        description="Roles bundle permissions. Users are assigned roles, never permissions directly."
        breadcrumbs={[{ label: "Admin", href: "/admin" }, { label: "Roles" }]}
        action={
          canWrite && (
            <Button leftSection={<Plus size={16} />} onClick={() => setDialog({ kind: "create" })}>
              New role
            </Button>
          )
        }
      />

      {loading && roles.length === 0 ? (
        <Stack gap="xs">
          {Array.from({ length: 3 }, (_, i) => (
            <Skeleton key={i} h={48} radius="sm" />
          ))}
        </Stack>
      ) : roles.length === 0 ? (
        <EmptyState icon={<ShieldOff size={28} />} title="No roles yet">
          Create a role to bundle permissions, then assign it to users.
        </EmptyState>
      ) : (
        <Table.ScrollContainer minWidth={720}>
          <Table verticalSpacing="sm" highlightOnHover>
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
                // Which roles are protected is data in Keycloak (a role attribute), not a name the app knows.
                const isProtected = !!role.isProtected;
                return (
                  <Table.Tr key={role.name}>
                    <Table.Td style={{ whiteSpace: "nowrap" }}>
                      <Group gap="xs" wrap="nowrap">
                        <Text fw={500}>{role.name}</Text>
                        {isProtected && (
                          // Badges truncate their label (overflow: hidden), which also lets the table size this
                          // column narrower than the label; keep the label's full width.
                          <Badge
                            size="sm"
                            variant="outline"
                            color="gray"
                            style={{ flexShrink: 0 }}
                            styles={{ label: { overflow: "visible", textOverflow: "clip" } }}
                          >
                            Protected
                          </Badge>
                        )}
                      </Group>
                    </Table.Td>
                    <Table.Td>{role.description}</Table.Td>
                    <Table.Td>
                      <Group gap={4}>
                        {(role.permissions ?? []).map((permission) => (
                          <Badge key={permission} variant="light" tt="none">
                            {permission}
                          </Badge>
                        ))}
                      </Group>
                    </Table.Td>
                    {canWrite && (
                      <Table.Td>
                        <Group gap={2} justify="flex-end" wrap="nowrap">
                          {isProtected ? (
                            <Tooltip label="Protected roles can't be modified or deleted">
                              <ActionIcon variant="subtle" color="gray" aria-label="Locked role" data-disabled>
                                <Lock size={18} />
                              </ActionIcon>
                            </Tooltip>
                          ) : (
                            <>
                              <Tooltip label="Edit">
                                <ActionIcon variant="subtle" aria-label={`Edit ${role.name}`} onClick={() => setDialog({ kind: "edit", role })}>
                                  <Pencil size={18} />
                                </ActionIcon>
                              </Tooltip>
                              <Tooltip label="Delete">
                                <ActionIcon
                                  variant="subtle"
                                  color="red"
                                  aria-label={`Delete ${role.name}`}
                                  onClick={() => setDialog({ kind: "delete", role })}
                                >
                                  <Trash2 size={18} />
                                </ActionIcon>
                              </Tooltip>
                            </>
                          )}
                        </Group>
                      </Table.Td>
                    )}
                  </Table.Tr>
                );
              })}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {(dialog?.kind === "create" || dialog?.kind === "edit") && (
        <RoleFormModal role={dialog.kind === "edit" ? dialog.role : undefined} catalog={catalog} onClose={close} />
      )}
      {dialog?.kind === "delete" && <DeleteRoleModal role={dialog.role} onClose={close} />}
    </>
  );
}

function RoleFormModal({ role, catalog, onClose }: { role?: IdentityRoleDto; catalog: IdentityPermissionDto[]; onClose: () => void }) {
  const isEdit = !!role;
  const [name, setName] = useState(role?.name ?? "");
  const [description, setDescription] = useState(role?.description ?? "");
  const [selected, setSelected] = useState<string[]>(role?.permissions ?? []);
  const [error, setError] = useState<string>();
  const saving = useUnit([createRoleFx.pending, updateRoleFx.pending]).some(Boolean);

  const submit = async () => {
    if (!name.trim()) {
      setError("Enter a role name.");
      return;
    }
    const params = { name: name.trim(), description: description.trim() || undefined, permissions: selected };
    try {
      if (isEdit) {
        await updateRoleFx(params);
        notifySuccess("Role updated", `Users holding ${params.name} get the new permissions at their next sign-in.`);
      } else {
        await createRoleFx(params);
        notifySuccess("Role created", `${params.name} can now be assigned to users.`);
      }
      onClose();
    } catch (e) {
      const fieldError = getFieldError(e, "Name");
      if (fieldError) setError(fieldError);
      else notifyError(isEdit ? "Couldn't update the role" : "Couldn't create the role", e);
    }
  };

  return (
    <Modal opened onClose={saving ? () => {} : onClose} title={isEdit ? `Edit "${role.name}"` : "New role"} size="lg">
      <Stack>
        <TextInput
          label="Name"
          description={isEdit ? "Role names can't be changed." : undefined}
          value={name}
          onChange={(e) => setName(e.currentTarget.value)}
          disabled={isEdit}
          error={error}
          data-autofocus={!isEdit || undefined}
        />
        <TextInput label="Description" value={description} onChange={(e) => setDescription(e.currentTarget.value)} />
        <Checkbox.Group label="Permissions" value={selected} onChange={setSelected}>
          <Stack gap="sm" mt="xs">
            {groupByResource(catalog).map(([resource, permissions]) => (
              <div key={resource}>
                <Text size="sm" fw={600} tt="capitalize" mb={4}>
                  {resource}
                </Text>
                <SimpleGrid cols={{ base: 1, xs: 2 }} spacing="xs">
                  {permissions.map((permission) => (
                    <Checkbox key={permission.name} value={permission.name} label={permission.name} description={permission.description} />
                  ))}
                </SimpleGrid>
              </div>
            ))}
          </Stack>
        </Checkbox.Group>
        {isEdit && (
          <Alert variant="light" color="blue" icon={<Info size={16} />} p="xs">
            Permission changes reach users who hold this role at their next sign-in.
          </Alert>
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

function DeleteRoleModal({ role, onClose }: { role: IdentityRoleDto; onClose: () => void }) {
  const saving = useUnit(deleteRoleFx.pending);

  const submit = async () => {
    if (!role.name) return;
    try {
      await deleteRoleFx(role.name);
      notifySuccess("Role deleted", `${role.name} was removed from every user who held it.`);
      onClose();
    } catch (e) {
      notifyError("Couldn't delete the role", e);
    }
  };

  return (
    <Modal opened onClose={saving ? () => {} : onClose} title={`Delete "${role.name}"?`}>
      <Stack>
        <Text>The role is removed from every user who holds it, and they lose its permissions.</Text>
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
