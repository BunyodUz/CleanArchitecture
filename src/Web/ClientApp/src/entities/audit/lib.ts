import type { MantineColor } from "@mantine/core";

interface ActionStyle {
  label: string;
  color: MantineColor;
}

// Mirrors src/Domain/Constants/AuditActions.cs.
const ACTIONS: Record<string, ActionStyle> = {
  "user.created": { label: "Created user", color: "green" },
  "user.updated": { label: "Updated user", color: "blue" },
  "user.deleted": { label: "Deleted user", color: "red" },
  "user.password_reset": { label: "Reset password", color: "orange" },
  "user.roles_changed": { label: "Changed roles", color: "violet" },
  "role.created": { label: "Created role", color: "green" },
  "role.updated": { label: "Updated role", color: "blue" },
  "role.deleted": { label: "Deleted role", color: "red" },
};

/** Display label and badge colour for an audit action such as "user.created". */
export const describeAuditAction = (action?: string): ActionStyle =>
  (action && ACTIONS[action]) || { label: action ?? "Unknown", color: "gray" };
