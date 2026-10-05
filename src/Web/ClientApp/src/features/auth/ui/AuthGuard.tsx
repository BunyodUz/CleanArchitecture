"use client";

import { Button } from "@mantine/core";
import { useUnit } from "effector-react";
import { House, Lock } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import type { ReactNode } from "react";
import { $isAuthenticated, $permissions, $sessionLoading } from "@/entities/session";
import { StatusPage } from "@/shared/ui/StatusPage";

interface AuthGuardProps {
  children: ReactNode;
  /** The caller must hold every one of these permissions. */
  requirePermissions?: string[];
  /** The caller must hold at least one of these permissions. */
  requireAnyPermission?: string[];
}

/**
 * Renders nothing while the session loads, sends signed-out visitors to sign in, and shows a
 * "no access" page (rather than silently redirecting) when a signed-in user lacks a permission.
 * The API enforces the same rules; this only decides what to show.
 */
export function AuthGuard({ children, requirePermissions = [], requireAnyPermission = [] }: AuthGuardProps) {
  const [isAuthenticated, permissions, isLoading] = useUnit([$isAuthenticated, $permissions, $sessionLoading]);
  const router = useRouter();
  const pathname = usePathname();

  const isAllowed =
    requirePermissions.every((p) => permissions.includes(p)) &&
    (requireAnyPermission.length === 0 || requireAnyPermission.some((p) => permissions.includes(p)));

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace(`/login?returnUrl=${encodeURIComponent(pathname)}`);
    }
  }, [isLoading, isAuthenticated, pathname, router]);

  if (isLoading || !isAuthenticated) {
    return null;
  }

  if (!isAllowed) {
    const needed = [...requirePermissions, ...requireAnyPermission];
    return (
      <StatusPage
        code="403"
        color="red"
        icon={<Lock size={28} />}
        title="You don't have access to this page"
        actions={
          <Button component={Link} href="/" leftSection={<House size={16} />}>
            Back to Home
          </Button>
        }
      >
        It needs {requireAnyPermission.length > 1 ? "one of these permissions" : "the permission"}:{" "}
        <b>{needed.join(", ")}</b>. Ask an administrator to give you a role that includes it.
      </StatusPage>
    );
  }

  return <>{children}</>;
}
