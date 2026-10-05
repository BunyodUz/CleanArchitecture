"use client";

import { Anchor, Badge, Button, Group, Paper, SimpleGrid, Skeleton, Stack, Text, Title, UnstyledButton } from "@mantine/core";
import { useUnit } from "effector-react";
import { History, ShieldCheck, Users } from "lucide-react";
import Link from "next/link";
import { useEffect } from "react";
import { $auditEntries, $auditLoading, describeAuditAction, fetchAuditLogFx } from "@/entities/audit";
import { $roles, $rolesLoading, fetchRolesFx } from "@/entities/role";
import { $permissions } from "@/entities/session";
import { $users, $usersLoading, fetchUsersFx } from "@/entities/user";
import { PERMISSIONS } from "@/shared/config/permissions";
import { formatDateTime, formatRelativeTime } from "@/shared/lib/time";
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

const RECENT_CHANGES = 5;

function RecentChanges() {
  const [entries, loading] = useUnit([$auditEntries, $auditLoading]);
  const recent = entries.slice(0, RECENT_CHANGES);

  return (
    <Paper withBorder radius="md" p="md" mt="md">
      <Group justify="space-between" mb="sm">
        <Title order={2} fz="h4">
          Recent changes
        </Title>
        <Anchor component={Link} href="/admin/audit" fz="sm">
          View all
        </Anchor>
      </Group>
      {loading && recent.length === 0 ? (
        <Stack gap="xs">
          {Array.from({ length: 3 }, (_, i) => (
            <Skeleton key={i} h={22} />
          ))}
        </Stack>
      ) : recent.length === 0 ? (
        <Text c="dimmed" fz="sm">
          No changes recorded yet.
        </Text>
      ) : (
        <Stack gap="xs">
          {recent.map((entry) => {
            const action = describeAuditAction(entry.action);
            return (
              <Group key={entry.id} justify="space-between" wrap="nowrap" gap="sm">
                <Group gap={8} wrap="wrap" miw={0}>
                  <Badge variant="light" color={action.color} tt="none" style={{ flexShrink: 0 }}>
                    {action.label}
                  </Badge>
                  <Text fz="sm" fw={500}>
                    {entry.targetName ?? entry.targetId}
                  </Text>
                  <Text fz="sm" c="dimmed">
                    by {entry.actorName ?? "unknown"}
                  </Text>
                </Group>
                {entry.timestamp && (
                  <Text fz="xs" c="dimmed" title={formatDateTime(entry.timestamp)} style={{ whiteSpace: "nowrap" }}>
                    {formatRelativeTime(entry.timestamp)}
                  </Text>
                )}
              </Group>
            );
          })}
        </Stack>
      )}
    </Paper>
  );
}

export function AdminOverviewView() {
  const [permissions, users, usersLoading, roles, rolesLoading] = useUnit([$permissions, $users, $usersLoading, $roles, $rolesLoading]);
  const canReadUsers = permissions.includes(PERMISSIONS.users.read);
  const canReadRoles = permissions.includes(PERMISSIONS.roles.read);
  const canReadAudit = permissions.includes(PERMISSIONS.audit.read);
  const protectedRoles = new Set(roles.filter((r) => r.isProtected).map((r) => r.name));

  useEffect(() => {
    if (canReadUsers) fetchUsersFx().catch(() => undefined);
    if (canReadRoles) fetchRolesFx().catch(() => undefined);
    if (canReadAudit) fetchAuditLogFx({ page: 1, pageSize: RECENT_CHANGES }).catch(() => undefined);
  }, [canReadUsers, canReadRoles, canReadAudit]);

  return (
    <>
      <PageHeader
        title="Overview"
        description="Users, roles and recent changes at a glance."
        breadcrumbs={[{ label: "Admin", href: "/admin" }, { label: "Overview" }]}
      />
      <SimpleGrid cols={{ base: 2, md: 4 }} spacing="md">
        {canReadUsers && (
          <>
            <Stat label="Users" value={users.length} href="/admin/users" loading={usersLoading && users.length === 0} />
            <Stat label="Disabled" value={users.filter((u) => !u.enabled).length} href="/admin/users" loading={usersLoading && users.length === 0} />
            {canReadRoles && (
              <Stat
                label="With a protected role"
                value={users.filter((u) => u.roles?.some((r) => protectedRoles.has(r))).length}
                href="/admin/users"
                loading={(usersLoading && users.length === 0) || (rolesLoading && roles.length === 0)}
              />
            )}
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
            {canReadAudit && (
              <Button component={Link} href="/admin/audit" variant="light" leftSection={<History size={16} />}>
                Audit log
              </Button>
            )}
          </Group>
        </Stack>
      </Paper>

      {canReadAudit && <RecentChanges />}
    </>
  );
}
