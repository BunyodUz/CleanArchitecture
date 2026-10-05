"use client";

import { Button, Group, Paper, SimpleGrid, Skeleton, Stack, Text, Title, UnstyledButton } from "@mantine/core";
import { useUnit } from "effector-react";
import { ShieldCheck, Users } from "lucide-react";
import Link from "next/link";
import { useEffect } from "react";
import { $roles, $rolesLoading, fetchRolesFx } from "@/entities/role";
import { $permissions } from "@/entities/session";
import { $users, $usersLoading, fetchUsersFx } from "@/entities/user";
import { ADMINISTRATOR_ROLE, PERMISSIONS } from "@/shared/config/permissions";
import { PageHeader } from "@/shared/ui/PageHeader";

function Stat({ label, value, href, loading }: { label: string; value: number; href: string; loading: boolean }) {
  return (
    <UnstyledButton component={Link} href={href}>
      <Paper withBorder radius="md" p="md">
        {loading ? <Skeleton h={30} w={50} mb={6} /> : (
          <Text fz={28} fw={700} lh={1.1} style={{ fontVariantNumeric: "tabular-nums" }}>
            {value}
          </Text>
        )}
        <Text c="dimmed" fz="sm">
          {label}
        </Text>
      </Paper>
    </UnstyledButton>
  );
}

export function AdminOverviewView() {
  const [permissions, users, usersLoading, roles, rolesLoading] = useUnit([$permissions, $users, $usersLoading, $roles, $rolesLoading]);
  const canReadUsers = permissions.includes(PERMISSIONS.users.read);
  const canReadRoles = permissions.includes(PERMISSIONS.roles.read);

  useEffect(() => {
    if (canReadUsers) fetchUsersFx();
    if (canReadRoles) fetchRolesFx();
  }, [canReadUsers, canReadRoles]);

  return (
    <>
      <PageHeader
        title="Overview"
        description="Users and roles at a glance."
        breadcrumbs={[{ label: "Admin", href: "/admin" }, { label: "Overview" }]}
      />
      <SimpleGrid cols={{ base: 2, md: 4 }} spacing="md">
        {canReadUsers && (
          <>
            <Stat label="Users" value={users.length} href="/admin/users" loading={usersLoading && users.length === 0} />
            <Stat label="Disabled" value={users.filter((u) => !u.enabled).length} href="/admin/users" loading={usersLoading && users.length === 0} />
            <Stat
              label="Administrators"
              value={users.filter((u) => u.roles?.includes(ADMINISTRATOR_ROLE)).length}
              href="/admin/users"
              loading={usersLoading && users.length === 0}
            />
          </>
        )}
        {canReadRoles && <Stat label="Roles" value={roles.length} href="/admin/roles" loading={rolesLoading && roles.length === 0} />}
      </SimpleGrid>

      <Paper withBorder radius="md" p="md" mt="md">
        <Stack gap="sm">
          <Title order={2} fz="h4">
            Manage
          </Title>
          <Group>
            {canReadUsers && (
              <Button component={Link} href="/admin/users" variant="light" leftSection={<Users size={16} />}>
                Users
              </Button>
            )}
            {canReadRoles && (
              <Button component={Link} href="/admin/roles" variant="light" leftSection={<ShieldCheck size={16} />}>
                Roles and permissions
              </Button>
            )}
          </Group>
        </Stack>
      </Paper>
    </>
  );
}
