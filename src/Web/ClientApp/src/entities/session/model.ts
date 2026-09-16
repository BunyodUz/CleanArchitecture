import { createEffect, createStore } from "effector";
import { accountClient } from "@/shared/api/client";
import { CurrentUserDto } from "@/web-api-client";

const ANONYMOUS = new CurrentUserDto({
  isAuthenticated: false,
  userName: undefined,
  roles: [],
  permissions: [],
});

export const fetchCurrentUserFx = createEffect(() =>
  accountClient.getCurrentUser().catch(() => ANONYMOUS),
);

export const $currentUser = createStore<CurrentUserDto>(ANONYMOUS).on(
  fetchCurrentUserFx.doneData,
  (_, user) => user,
);

export const $isAuthenticated = $currentUser.map((user) => user.isAuthenticated);
export const $permissions = $currentUser.map((user) => user.permissions);
export const $roles = $currentUser.map((user) => user.roles);
export const $isAdministrator = $roles.map((roles) => roles.includes("Administrator"));
export const $sessionLoading = fetchCurrentUserFx.pending;
