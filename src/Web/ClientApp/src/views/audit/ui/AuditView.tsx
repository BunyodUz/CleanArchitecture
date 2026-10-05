"use client";

import { Badge, Group, Pagination, Skeleton, Stack, Table, Text, Tooltip } from "@mantine/core";
import { useUnit } from "effector-react";
import { History } from "lucide-react";
import { useEffect, useState } from "react";
import { $auditEntries, $auditLoading, $auditTotal, describeAuditAction, fetchAuditLogFx } from "@/entities/audit";
import { notifyError } from "@/shared/lib/notify";
import { formatDateTime, formatRelativeTime } from "@/shared/lib/time";
import { EmptyState } from "@/shared/ui/EmptyState";
import { PageHeader } from "@/shared/ui/PageHeader";

const PAGE_SIZE = 20;

export function AuditView() {
  const [entries, total, loading] = useUnit([$auditEntries, $auditTotal, $auditLoading]);
  const [page, setPage] = useState(1);
  const pageCount = Math.max(1, Math.ceil(total / PAGE_SIZE));

  useEffect(() => {
    fetchAuditLogFx({ page, pageSize: PAGE_SIZE }).catch((e) => notifyError("Couldn't load the audit log", e));
  }, [page]);

  return (
    <>
      <PageHeader
        title="Audit log"
        description="Every change to users and roles, and the administrator who made it."
        breadcrumbs={[{ label: "Admin", href: "/admin" }, { label: "Audit log" }]}
      />

      {loading && entries.length === 0 ? (
        <Stack gap="xs">
          {Array.from({ length: 6 }, (_, i) => (
            <Skeleton key={i} h={40} radius="sm" />
          ))}
        </Stack>
      ) : entries.length === 0 ? (
        <EmptyState icon={<History size={28} />} title="No changes recorded yet">
          Creating, editing or deleting users and roles adds an entry here.
        </EmptyState>
      ) : (
        <Table.ScrollContainer minWidth={760}>
          <Table verticalSpacing="sm" highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>When</Table.Th>
                <Table.Th>Who</Table.Th>
                <Table.Th>Action</Table.Th>
                <Table.Th>Target</Table.Th>
                <Table.Th>Details</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {entries.map((entry) => {
                const action = describeAuditAction(entry.action);
                return (
                  <Table.Tr key={entry.id}>
                    <Table.Td style={{ whiteSpace: "nowrap" }}>
                      {entry.timestamp && (
                        <Tooltip label={formatDateTime(entry.timestamp)}>
                          <Text fz="sm" component="time" dateTime={entry.timestamp.toISOString()}>
                            {formatRelativeTime(entry.timestamp)}
                          </Text>
                        </Tooltip>
                      )}
                    </Table.Td>
                    <Table.Td>{entry.actorName ?? <Text c="dimmed">unknown</Text>}</Table.Td>
                    <Table.Td>
                      <Badge variant="light" color={action.color} tt="none" style={{ flexShrink: 0 }}>
                        {action.label}
                      </Badge>
                    </Table.Td>
                    <Table.Td>
                      <Text fw={500} fz="sm">
                        {entry.targetName ?? entry.targetId}
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <Text fz="sm" c="dimmed">
                        {entry.details}
                      </Text>
                    </Table.Td>
                  </Table.Tr>
                );
              })}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {pageCount > 1 && (
        <Group justify="flex-end" mt="md">
          <Pagination total={pageCount} value={page} onChange={setPage} size="sm" />
        </Group>
      )}
    </>
  );
}
