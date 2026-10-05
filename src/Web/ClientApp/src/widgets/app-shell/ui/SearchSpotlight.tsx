"use client";

import { ActionIcon, Group, Kbd, Text, UnstyledButton } from "@mantine/core";
import { useDebouncedValue, useOs } from "@mantine/hooks";
import { Spotlight, spotlight, type SpotlightActionGroupData } from "@mantine/spotlight";
import { useUnit } from "effector-react";
import { Search, UserRound } from "lucide-react";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState } from "react";
import { $isAuthenticated, $permissions } from "@/entities/session";
import { usersClient } from "@/shared/api/client";
import { PERMISSIONS } from "@/shared/config/permissions";
import type { IdentityUserDto } from "@/web-api-client";
import { searchablePages } from "../model";

const MAX_USERS = 5;

/**
 * Ctrl+K / ⌘K search: jumps to any page the user can open and, for admins with users.read,
 * to people found by the same server-side search the Users page uses.
 */
export function SearchSpotlight() {
  const [isAuthenticated, permissions] = useUnit([$isAuthenticated, $permissions]);
  const router = useRouter();
  const [query, setQuery] = useState("");
  const [debouncedQuery] = useDebouncedValue(query.trim(), 250);
  const [users, setUsers] = useState<{ query: string; items: IdentityUserDto[] }>({ query: "", items: [] });
  const canSearchUsers = isAuthenticated && permissions.includes(PERMISSIONS.users.read);
  const searchUsers = canSearchUsers && debouncedQuery.length >= 2;

  useEffect(() => {
    if (!searchUsers) return;
    let current = true;
    usersClient
      .getUsers(debouncedQuery)
      .then((items) => current && setUsers({ query: debouncedQuery, items: items.slice(0, MAX_USERS) }))
      .catch(() => undefined);
    return () => {
      current = false;
    };
  }, [searchUsers, debouncedQuery]);

  const actions = useMemo(() => {
    const groups = new Map<string, SpotlightActionGroupData>();
    for (const page of searchablePages(isAuthenticated, permissions)) {
      const group = groups.get(page.area) ?? { group: page.area, actions: [] };
      group.actions.push({
        id: `page:${page.href}`,
        label: page.label,
        description: page.href,
        leftSection: <page.icon size={18} />,
        onClick: () => router.push(page.href),
      });
      groups.set(page.area, group);
    }

    // Only show results for the query currently typed, never a previous one.
    const matchedUsers = searchUsers && users.query === debouncedQuery ? users.items : [];
    if (matchedUsers.length > 0) {
      groups.set("People", {
        group: "People",
        actions: matchedUsers.map((user) => ({
          id: `user:${user.id}`,
          label: user.username ?? "",
          description: [[user.firstName, user.lastName].filter(Boolean).join(" "), user.email].filter(Boolean).join(" · "),
          // The server already matched these; keywords keep the default filter from hiding them.
          keywords: [user.email ?? "", user.firstName ?? "", user.lastName ?? "", debouncedQuery],
          leftSection: <UserRound size={18} />,
          onClick: () => router.push(`/admin/users/?search=${encodeURIComponent(user.username ?? "")}`),
        })),
      });
    }

    return [...groups.values()];
  }, [isAuthenticated, permissions, router, searchUsers, users, debouncedQuery]);

  return (
    <Spotlight
      actions={actions}
      query={query}
      onQueryChange={setQuery}
      shortcut={["mod + K", "/"]}
      nothingFound="Nothing found"
      highlightQuery
      scrollable
      maxHeight={420}
      searchProps={{
        leftSection: <Search size={18} />,
        placeholder: canSearchUsers ? "Search pages and people…" : "Search pages…",
      }}
    />
  );
}

/** Top-bar trigger: a search field look-alike on desktop, an icon button on phones. */
export function SearchButton() {
  const os = useOs();
  const shortcut = os === "macos" || os === "ios" ? "⌘ K" : "Ctrl K";

  return (
    <>
      <UnstyledButton
        visibleFrom="sm"
        onClick={spotlight.open}
        aria-label="Search"
        px="sm"
        h={34}
        w={240}
        style={{
          border: "1px solid var(--mantine-color-default-border)",
          borderRadius: "var(--mantine-radius-md)",
          background: "var(--mantine-color-default)",
        }}
      >
        <Group justify="space-between" wrap="nowrap" gap="xs">
          <Group gap={8} wrap="nowrap">
            <Search size={16} color="var(--mantine-color-dimmed)" />
            <Text fz="sm" c="dimmed">
              Search…
            </Text>
          </Group>
          <Kbd size="xs">{shortcut}</Kbd>
        </Group>
      </UnstyledButton>
      <ActionIcon hiddenFrom="sm" variant="subtle" color="gray" size="lg" onClick={spotlight.open} aria-label="Search">
        <Search size={18} />
      </ActionIcon>
    </>
  );
}
