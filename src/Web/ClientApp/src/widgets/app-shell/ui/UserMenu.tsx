"use client";

import { Avatar, Badge, Group, Menu, Stack, Text, UnstyledButton } from "@mantine/core";
import { useUnit } from "effector-react";
import { ArrowLeft, ChevronDown, ChevronRight, LogOut, Shield, UserRound } from "lucide-react";
import Link from "next/link";
import { $currentUser } from "@/entities/session";
import { logout } from "@/features/auth";
import { ADMIN_PERMISSIONS, type ShellVariant } from "../model";

const initialsOf = (name: string) =>
  name
    .split(/[\s._-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]!.toUpperCase())
    .join("") || "?";

export function UserMenu({ variant }: { variant: ShellVariant }) {
  const user = useUnit($currentUser);
  const displayName = user.fullName ?? user.userName ?? "";
  const canAdminister = ADMIN_PERMISSIONS.some((p) => user.permissions.includes(p));

  return (
    <Menu position="bottom-end" width={260} shadow="md" withinPortal>
      <Menu.Target>
        <UnstyledButton aria-label="Account menu" style={{ borderRadius: 999, padding: 3 }}>
          <Group gap={4} wrap="nowrap">
            <Avatar size={32} radius="xl" color="blue">
              {initialsOf(displayName)}
            </Avatar>
            <ChevronDown size={16} />
          </Group>
        </UnstyledButton>
      </Menu.Target>
      <Menu.Dropdown>
        <Stack gap={2} px="sm" py={8}>
          <Text fw={600} fz="sm" truncate>
            {displayName}
          </Text>
          {user.email && (
            <Text fz="xs" c="dimmed" truncate>
              {user.email}
            </Text>
          )}
          {user.roles.length > 0 && (
            <Group gap={4} mt={4}>
              {user.roles.map((role) => (
                <Badge key={role} size="xs" variant="light">
                  {role}
                </Badge>
              ))}
            </Group>
          )}
        </Stack>
        <Menu.Divider />
        <Menu.Item component={Link} href="/profile" leftSection={<UserRound size={16} />}>
          Profile
        </Menu.Item>
        {canAdminister &&
          (variant === "admin" ? (
            <Menu.Item component={Link} href="/" leftSection={<ArrowLeft size={16} />}>
              Back to app
            </Menu.Item>
          ) : (
            <Menu.Item component={Link} href="/admin" leftSection={<Shield size={16} />} rightSection={<ChevronRight size={14} />}>
              Admin
            </Menu.Item>
          ))}
        <Menu.Divider />
        <Menu.Item color="red" leftSection={<LogOut size={16} />} onClick={() => logout()}>
          Log out
        </Menu.Item>
      </Menu.Dropdown>
    </Menu>
  );
}
