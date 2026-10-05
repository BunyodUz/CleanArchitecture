"use client";

import { NavLink, Stack, Text, Tooltip } from "@mantine/core";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { isActive, type NavItem, type ShellVariant } from "../model";

interface SideNavProps {
  items: NavItem[];
  variant: ShellVariant;
  /** Icon-only rail: labels move into tooltips. */
  iconOnly: boolean;
  onNavigate?: () => void;
}

export function SideNav({ items, variant, iconOnly, onNavigate }: SideNavProps) {
  const pathname = usePathname();

  return (
    <Stack gap={2}>
      <Text
        fz={11}
        fw={700}
        tt="uppercase"
        c="dimmed"
        px={10}
        pt={4}
        pb={6}
        style={{ letterSpacing: "0.07em", visibility: iconOnly ? "hidden" : "visible", whiteSpace: "nowrap" }}
      >
        {variant === "admin" ? "Administration" : "Workspace"}
      </Text>
      {items.map((item) => {
        const Icon = item.icon;
        const link = (
          <NavLink
            key={item.href}
            component={Link}
            href={item.href}
            onClick={onNavigate}
            active={isActive(item, pathname)}
            color={variant === "admin" ? "violet" : "blue"}
            label={iconOnly ? undefined : item.label}
            aria-label={item.label}
            leftSection={<Icon size={18} />}
            styles={{
              root: { borderRadius: "var(--mantine-radius-sm)", ...(iconOnly && { justifyContent: "center", paddingInline: 0 }) },
              section: iconOnly ? { marginInlineEnd: 0 } : undefined,
              body: iconOnly ? { display: "none" } : undefined,
            }}
          />
        );
        return iconOnly ? (
          <Tooltip key={item.href} label={item.label} position="right" withArrow openDelay={150}>
            {link}
          </Tooltip>
        ) : (
          link
        );
      })}
    </Stack>
  );
}
