import { createEffect, createStore } from "effector";
import { auditLogClient } from "@/shared/api/client";
import type { AuditEntryDto } from "@/web-api-client";

export interface AuditPageParams {
  page: number;
  pageSize: number;
}

export const fetchAuditLogFx = createEffect(({ page, pageSize }: AuditPageParams) =>
  auditLogClient.getAuditLog(page, pageSize),
);

export const $auditEntries = createStore<AuditEntryDto[]>([]).on(fetchAuditLogFx.doneData, (_, page) => page.items ?? []);
export const $auditTotal = createStore(0).on(fetchAuditLogFx.doneData, (_, page) => page.totalCount ?? 0);
export const $auditLoading = fetchAuditLogFx.pending;
