"use client";

import { useUnit } from "effector-react";
import { usePathname, useRouter } from "next/navigation";
import { useEffect } from "react";
import type { ReactNode } from "react";
import { $isAuthenticated, $sessionLoading } from "@/entities/session";

// Mirrors the old react-router ProtectedRoute: renders nothing while the session is loading or
// while redirecting, and only reveals its children once the session is confirmed authenticated.
export function AuthGuard({ children }: { children: ReactNode }) {
  const [isAuthenticated, isLoading] = useUnit([$isAuthenticated, $sessionLoading]);
  const router = useRouter();
  const pathname = usePathname();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.replace(`/login?returnUrl=${encodeURIComponent(pathname)}`);
    }
  }, [isLoading, isAuthenticated, pathname, router]);

  if (isLoading || !isAuthenticated) {
    return null;
  }

  return <>{children}</>;
}
