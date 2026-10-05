"use client";

import { Alert, Avatar, Badge, Button, Grid, Group, Paper, SimpleGrid, Stack, Text, ThemeIcon, Title } from "@mantine/core";
import { useUnit } from "effector-react";
import { Check, ExternalLink, Info, Minus } from "lucide-react";
import { $currentUser } from "@/entities/session";
import { PERMISSION_DESCRIPTIONS } from "@/shared/config/permissions";
import { PageHeader } from "@/shared/ui/PageHeader";

const resourceOf = (permission: string) => permission.split(".")[0];
const RESOURCE_LABELS: Record<string, string> = { todolists: "Todo lists", todoitems: "Tasks", users: "Users", roles: "Roles" };

export function ProfileView() {
  const user = useUnit($currentUser);
  const displayName = user.fullName ?? user.userName ?? "";
  const catalog = Object.keys(PERMISSION_DESCRIPTIONS);
  const resources = [...new Set(catalog.map(resourceOf))];

  return (
    <>
      <PageHeader
        title="Profile"
        description="Your account and what it lets you do."
        breadcrumbs={[{ label: "Home", href: "/" }, { label: "Profile" }]}
      />
      <Grid gap="md">
        <Grid.Col span={{ base: 12, md: 4 }}>
          <Paper withBorder radius="md" p="lg" h="100%">
            <Stack align="flex-start" gap="sm">
              <Avatar size={64} radius="xl" color="blue">
                {displayName.slice(0, 2).toUpperCase()}
              </Avatar>
              <div>
                <Title order={2} fz="h3">
                  {displayName}
                </Title>
                {user.fullName && <Text c="dimmed">{user.userName}</Text>}
                {user.email && <Text c="dimmed">{user.email}</Text>}
              </div>
              <Group gap={4}>
                {user.roles.map((role) => (
                  <Badge key={role} variant="light">
                    {role}
                  </Badge>
                ))}
              </Group>
              {user.manageAccountUrl && (
                <>
                  <Button
                    component="a"
                    href={user.manageAccountUrl}
                    target="_blank"
                    rel="noreferrer"
                    variant="default"
                    leftSection={<ExternalLink size={16} />}
                  >
                    Manage account
                  </Button>
                  <Text fz="sm" c="dimmed">
                    Change your password, set up two-factor sign-in, and see where you&apos;re signed in. Opens in Keycloak.
                  </Text>
                </>
              )}
            </Stack>
          </Paper>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 8 }}>
          <Paper withBorder radius="md" p="lg">
            <Stack gap="md">
              <Title order={2} fz="h4">
                What you can do
              </Title>
              <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="lg">
                {resources.map((resource) => (
                  <Stack key={resource} gap={6}>
                    <Text fw={600} fz="sm">
                      {RESOURCE_LABELS[resource] ?? resource}
                    </Text>
                    {catalog
                      .filter((p) => resourceOf(p) === resource)
                      .map((permission) => {
                        const granted = user.permissions.includes(permission);
                        return (
                          <Group key={permission} gap="sm" wrap="nowrap" align="flex-start">
                            <ThemeIcon size={20} radius="xl" variant={granted ? "filled" : "light"} color={granted ? "green" : "gray"}>
                              {granted ? <Check size={12} /> : <Minus size={12} />}
                            </ThemeIcon>
                            <div>
                              <Text fz="sm" c={granted ? undefined : "dimmed"}>
                                {PERMISSION_DESCRIPTIONS[permission]}
                              </Text>
                              <Text fz="xs" c="dimmed" ff="monospace">
                                {permission}
                              </Text>
                            </div>
                          </Group>
                        );
                      })}
                  </Stack>
                ))}
              </SimpleGrid>
              <Alert variant="light" icon={<Info size={16} />}>
                These come from your roles. If something you need is missing, ask an administrator for a role that includes it.
                Changes to your roles apply the next time you sign in.
              </Alert>
            </Stack>
          </Paper>
        </Grid.Col>
      </Grid>
    </>
  );
}
