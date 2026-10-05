import { createEffect, createStore } from "effector";
import { rolesClient } from "@/shared/api/client";
import { CreateRoleCommand, IdentityPermissionDto, IdentityRoleDto, UpdateRoleCommand } from "@/web-api-client";

export const fetchRolesFx = createEffect(() => rolesClient.getRoles());
export const fetchPermissionsFx = createEffect(() => rolesClient.getPermissions());

export const $roles = createStore<IdentityRoleDto[]>([]).on(fetchRolesFx.doneData, (_, roles) => roles);
export const $permissionCatalog = createStore<IdentityPermissionDto[]>([]).on(
  fetchPermissionsFx.doneData,
  (_, permissions) => permissions,
);
export const $rolesLoading = fetchRolesFx.pending;

export interface RoleParams {
  name: string;
  description?: string;
  permissions: string[];
}

export const createRoleFx = createEffect((params: RoleParams) => rolesClient.createRole(new CreateRoleCommand(params)));

export const updateRoleFx = createEffect((params: RoleParams) =>
  rolesClient.updateRole(params.name, new UpdateRoleCommand(params)),
);

export const deleteRoleFx = createEffect((name: string) => rolesClient.deleteRole(name));

const toRole = (params: RoleParams, existing?: IdentityRoleDto) =>
  new IdentityRoleDto({ ...existing, name: params.name, description: params.description, permissions: params.permissions });

$roles.on(createRoleFx.done, (roles, { params }) =>
  [...roles, toRole(params)].sort((a, b) => (a.name ?? "").localeCompare(b.name ?? "")),
);

$roles.on(updateRoleFx.done, (roles, { params }) =>
  roles.map((r) => (r.name === params.name ? toRole(params, r) : r)),
);

$roles.on(deleteRoleFx.done, (roles, { params: name }) => roles.filter((r) => r.name !== name));
