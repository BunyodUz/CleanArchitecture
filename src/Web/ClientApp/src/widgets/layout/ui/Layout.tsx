"use client";

import { AppShell } from "@mantine/core";
import type { ReactNode } from "react";
import { NavMenu } from "@/widgets/nav-menu";

export function Layout({ children }: { children: ReactNode }) {
  return (
    <AppShell header={{ height: 60 }} padding="md">
      <AppShell.Header>
        <NavMenu />
      </AppShell.Header>
      <AppShell.Main>{children}</AppShell.Main>
    </AppShell>
  );
}
