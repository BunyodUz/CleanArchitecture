import { API_BASE_URL } from "@/shared/config/env";

// Keycloak's hosted pages are the login/registration UI — these are full-page redirects to
// the backend's BFF endpoints (a different origin in dev), not client-side Next.js navigation
// or API calls, so router.push() / Effector don't apply here.
export function login(returnUrl: string = "/"): void {
  // eslint-disable-next-line @next/next/no-location-assign-relative-destination
  window.location.href = `${API_BASE_URL}/account/login?returnUrl=${encodeURIComponent(returnUrl)}`;
}

export function logout(): void {
  // eslint-disable-next-line @next/next/no-location-assign-relative-destination
  window.location.href = `${API_BASE_URL}/account/logout?returnUrl=${encodeURIComponent("/")}`;
}
