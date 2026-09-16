import { createEffect, createStore } from "effector";
import { usersClient } from "@/shared/api/client";
import {
  CreateUserCommand,
  IdentityRoleDto,
  IdentityUserDto,
  ResetPasswordCommand,
  SetUserRolesCommand,
  UpdateUserCommand,
} from "@/web-api-client";

export const fetchUsersFx = createEffect((search?: string) => usersClient.getUsers(search));
export const fetchRolesFx = createEffect(() => usersClient.getRoles());

export const $users = createStore<IdentityUserDto[]>([]).on(fetchUsersFx.doneData, (_, users) => users);
export const $availableRoles = createStore<IdentityRoleDto[]>([]).on(fetchRolesFx.doneData, (_, roles) => roles);
export const $usersLoading = fetchUsersFx.pending;

export interface CreateUserParams {
  username: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  password?: string;
  temporaryPassword: boolean;
  roles: string[];
}

export const createUserFx = createEffect((params: CreateUserParams) =>
  usersClient.createUser(new CreateUserCommand(params)),
);

export interface UpdateUserParams {
  id: string;
  username: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  enabled: boolean;
}

export const updateUserFx = createEffect((params: UpdateUserParams) =>
  usersClient.updateUser(params.id, new UpdateUserCommand(params)),
);

export const deleteUserFx = createEffect((id: string) => usersClient.deleteUser(id));

export interface ResetPasswordParams {
  id: string;
  password: string;
  temporary: boolean;
}

export const resetPasswordFx = createEffect((params: ResetPasswordParams) =>
  usersClient.resetPassword(params.id, new ResetPasswordCommand(params)),
);

export interface SetUserRolesParams {
  id: string;
  roles: string[];
}

export const setUserRolesFx = createEffect((params: SetUserRolesParams) =>
  usersClient.setUserRoles(params.id, new SetUserRolesCommand(params)),
);

$users.on(createUserFx.done, (users, { params, result: id }) => [
  ...users,
  new IdentityUserDto({
    id,
    username: params.username,
    email: params.email,
    firstName: params.firstName,
    lastName: params.lastName,
    enabled: true,
    roles: params.roles,
  }),
]);

$users.on(updateUserFx.done, (users, { params }) =>
  users.map((u) =>
    u.id === params.id
      ? new IdentityUserDto({
          ...u,
          username: params.username,
          email: params.email,
          firstName: params.firstName,
          lastName: params.lastName,
          enabled: params.enabled,
        })
      : u,
  ),
);

$users.on(deleteUserFx.done, (users, { params: id }) => users.filter((u) => u.id !== id));

$users.on(setUserRolesFx.done, (users, { params }) =>
  users.map((u) => (u.id === params.id ? new IdentityUserDto({ ...u, roles: params.roles }) : u)),
);
