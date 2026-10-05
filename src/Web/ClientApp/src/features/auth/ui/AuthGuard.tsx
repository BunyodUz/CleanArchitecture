"use client";

import { useUnit } from "effector-react";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import type { ReactNode } from "react";
import { $isAuthenticated, $permissions, $sessionLoading } from "@/entities/session";

interface AuthGuardProps {
  children: ReactNode;
  /** When set, the caller must also hold this permission (e.g. "users.read"), or they're
   * redirected home instead of being shown the page. */
  requirePermission?: string;
}

// Mirrors the old react-router ProtectedRoute: renders nothing while the session is loading or
// while redirecting, and only reveals its children once the session is confirmed authenticated
// (and, when requirePermission is set, the user holds that permission).
export function AuthGuard({ children, requirePermission }: AuthGuardProps) {
  const [isAuthenticated, permissions, isLoading] = useUnit([$isAuthenticated, $permissions, $sessionLoading]);
  const router = useRouter();
  const pathname = usePathname();

  const isAllowed = !requirePermission || permissions.includes(requirePermission);

  useEffect(() => {
    if (isLoading) return;

    if (!isAuthenticated) {
      router.replace(`/login?returnUrl=${encodeURIComponent(pathname)}`);
      return;
    }

    if (!isAllowed) {
      router.replace("/");
    }
  }, [isLoading, isAuthenticated, isAllowed, pathname, router]);

  if (isLoading || !isAuthenticated || !isAllowed) {
    return null;
  }

  return <>{children}</>;
}
