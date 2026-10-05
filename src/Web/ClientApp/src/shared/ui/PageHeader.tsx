"use client";

import { Anchor, Breadcrumbs, Group, Stack, Text, Title } from "@mantine/core";
import Link from "next/link";
import type { ReactNode } from "react";

export interface Crumb {
  label: string;
  href?: string;
}

interface PageHeaderProps {
  title: string;
  description?: ReactNode;
  breadcrumbs?: Crumb[];
  /** Primary action, shown on the right of the title row. */
  action?: ReactNode;
}

export function PageHeader({ title, description, breadcrumbs, action }: PageHeaderProps) {
  return (
    <Stack gap={4} mb="lg">
      {breadcrumbs && (
        <Breadcrumbs fz="sm" separatorMargin={6}>
          {breadcrumbs.map((crumb) =>
            crumb.href ? (
              <Anchor key={crumb.label} component={Link} href={crumb.href} fz="sm">
                {crumb.label}
              </Anchor>
            ) : (
              <Text key={crumb.label} fz="sm" c="dimmed">
                {crumb.label}
              </Text>
            ),
          )}
        </Breadcrumbs>
      )}
      <Group justify="space-between" align="flex-start" gap="sm">
        <div>
          <Title order={1}>{title}</Title>
          {description && <Text c="dimmed">{description}</Text>}
        </div>
        {action}
      </Group>
    </Stack>
  );
}
