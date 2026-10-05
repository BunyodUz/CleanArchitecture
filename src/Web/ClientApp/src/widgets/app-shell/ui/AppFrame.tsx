"use client";

import { ActionIcon, AppShell, Badge, Burger, Button, Group, Text, ThemeIcon, Tooltip, UnstyledButton } from "@mantine/core";
import { useDisclosure, useMediaQuery } from "@mantine/hooks";
import { useUnit } from "effector-react";
import { LogIn, PanelLeftClose, PanelLeftOpen } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { $isAuthenticated, $permissions, $sessionLoading } from "@/entities/session";
import { login } from "@/features/auth";
import { $sidebarCollapsed, sidebarToggled, UiSettingsMenu } from "@/features/ui-settings";
import { visibleNav, type ShellVariant } from "../model";
import { SearchButton, SearchSpotlight } from "./SearchSpotlight";
import { SideNav } from "./SideNav";
import { UserMenu } from "./UserMenu";

const EXPANDED_WIDTH = 240;
const COLLAPSED_WIDTH = 64;

/**
 * The page frame shared by the main app and the admin area: a top bar (menu toggle, brand,
 * search, UI settings, account) and a left sidebar that collapses to an icon rail on desktop and slides
 * in over the page on phones. The two areas differ only in their nav items and accent.
 */
export function AppFrame({ variant, children }: { variant: ShellVariant; children: ReactNode }) {
  const [isAuthenticated, permissions, sessionLoading, collapsed, toggleCollapsed] = useUnit([
    $isAuthenticated,
    $permissions,
    $sessionLoading,
    $sidebarCollapsed,
    sidebarToggled,
  ]);
  const [mobileOpened, mobile] = useDisclosure(false);
  const pathname = usePathname();
  const isDesktop = useMediaQuery("(min-width: 48em)") ?? true;
  const iconOnly = collapsed && isDesktop;
  const isAdmin = variant === "admin";

  // Close the phone menu after navigating.
  useEffect(() => mobile.close(), [pathname]); // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <AppShell
      header={{ height: 56 }}
      navbar={{
        width: collapsed ? COLLAPSED_WIDTH : EXPANDED_WIDTH,
        breakpoint: "sm",
        collapsed: { mobile: !mobileOpened },
      }}
      padding="md"
      transitionDuration={180}
    >
      <AppShell.Header>
        <Group h="100%" px="sm" justify="space-between" wrap="nowrap">
          <Group gap="xs" wrap="nowrap" miw={0}>
            <Burger opened={mobileOpened} onClick={mobile.toggle} hiddenFrom="sm" size="sm" aria-label="Open navigation" />
            <Tooltip label={collapsed ? "Expand sidebar" : "Collapse sidebar"} openDelay={400}>
              <ActionIcon
                visibleFrom="sm"
                variant="subtle"
                color="gray"
                size="lg"
                onClick={toggleCollapsed}
                aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
              >
                {collapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
              </ActionIcon>
            </Tooltip>
            <UnstyledButton component={Link} href={isAdmin ? "/admin" : "/"} aria-label="Home" miw={0}>
              <Group gap={8} wrap="nowrap">
                <ThemeIcon size={28} radius="md" color="blue">
                  <Text fz={11} fw={800} c="white">
                    CA
                  </Text>
                </ThemeIcon>
                <Text fw={700} truncate visibleFrom="xs">
                  Clean Architecture
                </Text>
              </Group>
            </UnstyledButton>
            {isAdmin && (
              <Badge color="violet" variant="light" radius="sm">
                Admin
              </Badge>
            )}
          </Group>

          <Group gap={6} wrap="nowrap">
            <SearchButton />
            <UiSettingsMenu />
            {isAuthenticated ? (
              <UserMenu variant={variant} />
            ) : (
              !sessionLoading && (
                <Button size="sm" leftSection={<LogIn size={16} />} onClick={() => login(pathname)}>
                  Log in
                </Button>
              )
            )}
          </Group>
        </Group>
      </AppShell.Header>

      <AppShell.Navbar
        p="xs"
        bg={isAdmin ? "var(--mantine-color-violet-light)" : undefined}
        style={{ transition: "width 180ms ease" }}
      >
        <SideNav
          items={visibleNav(variant, isAuthenticated, permissions)}
          variant={variant}
          iconOnly={iconOnly}
          onNavigate={mobile.close}
        />
      </AppShell.Navbar>

      <AppShell.Main>{children}</AppShell.Main>

      <SearchSpotlight />
    </AppShell>
  );
}
