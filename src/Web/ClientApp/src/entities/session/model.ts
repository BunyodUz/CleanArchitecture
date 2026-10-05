import { createEffect, createStore, sample } from "effector";
import { accountClient, apiUnauthorized } from "@/shared/api/client";
import { CurrentUserDto } from "@/web-api-client";

const ANONYMOUS = new CurrentUserDto({
  isAuthenticated: false,
  userName: undefined,
  email: undefined,
  fullName: undefined,
  roles: [],
  permissions: [],
  manageAccountUrl: undefined,
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
// Starts true, not fetchCurrentUserFx.pending (which is false until the fetch begins): on a
// direct load of a protected page, AuthGuard's first render would otherwise read "not loading,
// not authenticated" and redirect to login before the session request has even been sent.
export const $sessionLoading = createStore(true).on(fetchCurrentUserFx.finally, () => false);

/** True once an API call is rejected with 401 while we believed the user was signed in. */
export const $sessionExpired = createStore(false);

sample({
  clock: apiUnauthorized,
  source: $isAuthenticated,
  filter: (isAuthenticated) => isAuthenticated,
  fn: () => true,
  target: $sessionExpired,
});
