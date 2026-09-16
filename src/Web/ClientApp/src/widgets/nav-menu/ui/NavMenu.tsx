"use client";

import { Anchor, Group } from "@mantine/core";
import { useUnit } from "effector-react";
import Link from "next/link";
import { $isAdministrator, $isAuthenticated } from "@/entities/session";
import { logout } from "@/features/auth";
import { ThemeToggle } from "@/shared/ui/ThemeToggle";

function UsersLink() {
  const isAdministrator = useUnit($isAdministrator);

  if (!isAdministrator) return null;

  return (
    <Anchor component={Link} href="/users">
      Users
    </Anchor>
  );
}

function AuthLinks() {
  const isAuthenticated = useUnit($isAuthenticated);

  if (isAuthenticated) {
    return (
      <Anchor
        href="#"
        onClick={(e) => {
          e.preventDefault();
          logout();
        }}
      >
        Log out
      </Anchor>
    );
  }

  return (
    <Anchor component={Link} href="/login">
      Log in
    </Anchor>
  );
}

export function NavMenu() {
  return (
    <Group justify="space-between" px="md" h="100%" wrap="nowrap" component="nav">
      <Anchor component={Link} href="/" fw={700} underline="never">
        Clean Architecture
      </Anchor>
      <Group visibleFrom="xs">
        <Anchor component={Link} href="/">
          Home
        </Anchor>
        <Anchor component={Link} href="/counter">
          Counter
        </Anchor>
        <Anchor component={Link} href="/weather">
          Weather
        </Anchor>
        <Anchor component={Link} href="/todo">
          Tasks
        </Anchor>
        <UsersLink />
      </Group>
      <Group gap="md" wrap="nowrap">
        <AuthLinks />
        <ThemeToggle />
      </Group>
    </Group>
  );
}
