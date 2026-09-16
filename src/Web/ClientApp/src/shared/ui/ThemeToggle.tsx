"use client";

import { ActionIcon, useMantineColorScheme } from "@mantine/core";
import { Laptop, Moon, Sun } from "lucide-react";

const NEXT_SCHEME = { auto: "light", light: "dark", dark: "auto" } as const;
const ICONS = { auto: Laptop, light: Sun, dark: Moon } as const;

export function ThemeToggle() {
  const { colorScheme, setColorScheme } = useMantineColorScheme();
  const Icon = ICONS[colorScheme];

  return (
    <ActionIcon
      variant="subtle"
      aria-label={`Switch theme (current: ${colorScheme})`}
      onClick={() => setColorScheme(NEXT_SCHEME[colorScheme])}
    >
      <Icon size={20} strokeWidth={2} />
    </ActionIcon>
  );
}
