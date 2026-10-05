"use client";

import { useUnit } from "effector-react";
import type { ReactNode } from "react";
import { $permissions } from "@/entities/session";
import { AuthGuard } from "@/features/auth";
import { ADMIN_PERMISSIONS, AppFrame } from "@/widgets/app-shell";

// People without any admin permission see the "no access" page inside the main app frame,
// not an admin-styled frame for an area they can't use.
export default function AdminLayout({ children }: { children: ReactNode }) {
  const permissions = useUnit($permissions);
  const canAdminister = ADMIN_PERMISSIONS.some((p) => permissions.includes(p));

  return (
    <AppFrame variant={canAdminister ? "admin" : "main"}>
      <AuthGuard requireAnyPermission={ADMIN_PERMISSIONS}>{children}</AuthGuard>
    </AppFrame>
  );
}
