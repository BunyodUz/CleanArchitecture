import { Paper, Stack, Text } from "@mantine/core";
import type { ReactNode } from "react";

interface EmptyStateProps {
  icon: ReactNode;
  title: string;
  children?: ReactNode;
  action?: ReactNode;
}

/** In-page "nothing here yet" / "nothing matches" placeholder for lists and tables. */
export function EmptyState({ icon, title, children, action }: EmptyStateProps) {
  return (
    <Paper withBorder radius="md" p="xl" style={{ borderStyle: "dashed" }}>
      <Stack align="center" gap={6} ta="center">
        <Text c="dimmed">{icon}</Text>
        <Text fw={600}>{title}</Text>
        {children && (
          <Text c="dimmed" fz="sm" maw={420}>
            {children}
          </Text>
        )}
        {action}
      </Stack>
    </Paper>
  );
}
