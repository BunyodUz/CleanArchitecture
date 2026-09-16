"use client";

import { useUnit } from "effector-react";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import type { ReactNode } from "react";
import { $isAuthenticated, $roles, $sessionLoading } from "@/entities/session";

interface AuthGuardProps {
  children: ReactNode;
  /** When set, the caller must also hold this realm role (e.g. "Administrator"), or they're
   * redirected home instead of being shown the page. */
  requireRole?: string;
}

// Mirrors the old react-router ProtectedRoute: renders nothing while the session is loading or
// while redirecting, and only reveals its children once the session is confirmed authenticated
// (and, when requireRole is set, the user holds that role).
export function AuthGuard({ children, requireRole }: AuthGuardProps) {
  const [isAuthenticated, roles, isLoading] = useUnit([$isAuthenticated, $roles, $sessionLoading]);
  const router = useRouter();
  const pathname = usePathname();

  const hasRequiredRole = !requireRole || roles.includes(requireRole);

  useEffect(() => {
    if (isLoading) return;

    if (!isAuthenticated) {
      router.replace(`/login?returnUrl=${encodeURIComponent(pathname)}`);
      return;
    }

    if (!hasRequiredRole) {
      router.replace("/");
    }
  }, [isLoading, isAuthenticated, hasRequiredRole, pathname, router]);

  if (isLoading || !isAuthenticated || !hasRequiredRole) {
    return null;
  }

  return <>{children}</>;
}
