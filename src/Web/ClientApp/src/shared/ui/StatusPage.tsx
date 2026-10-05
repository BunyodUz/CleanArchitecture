"use client";

import { Group, Stack, Text, ThemeIcon, Title } from "@mantine/core";
import type { ReactNode } from "react";

interface StatusPageProps {
  icon: ReactNode;
  title: string;
  children: ReactNode;
  /** Small label above the icon, e.g. "404". */
  code?: string;
  color?: string;
  actions?: ReactNode;
}

/** Full-page state for "not found", "no access", "sign in required" and similar. */
export function StatusPage({ icon, title, children, code, color = "blue", actions }: StatusPageProps) {
  return (
    <Stack align="center" justify="center" gap="sm" mih={420} ta="center" px="md">
      {code && (
        <Text ff="monospace" fw={700} fz="xs" c="dimmed" style={{ letterSpacing: "0.1em" }}>
          {code}
        </Text>
      )}
      <ThemeIcon size={60} radius="xl" variant="light" color={color}>
        {icon}
      </ThemeIcon>
      <Title order={1} fz="h2">
        {title}
      </Title>
      <Text c="dimmed" maw={460}>
        {children}
      </Text>
      {actions && <Group justify="center">{actions}</Group>}
    </Stack>
  );
}
