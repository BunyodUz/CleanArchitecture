import { CloudSun, Hash, House, LayoutDashboard, ListChecks, ShieldCheck, Users, type LucideIcon } from "lucide-react";
import { PERMISSIONS } from "@/shared/config/permissions";

export type ShellVariant = "main" | "admin";

export interface NavItem {
  href: string;
  label: string;
  icon: LucideIcon;
  /** Shown to signed-out visitors too. */
  public?: boolean;
  /** Hidden unless the user holds this permission. */
  permission?: string;
}

export const MAIN_NAV: NavItem[] = [
  { href: "/", label: "Home", icon: House, public: true },
  { href: "/todo", label: "Tasks", icon: ListChecks, permission: PERMISSIONS.todoLists.read },
  { href: "/weather", label: "Weather", icon: CloudSun },
  { href: "/counter", label: "Counter", icon: Hash, public: true },
];

export const ADMIN_NAV: NavItem[] = [
  { href: "/admin", label: "Overview", icon: LayoutDashboard },
  { href: "/admin/users", label: "Users", icon: Users, permission: PERMISSIONS.users.read },
  { href: "/admin/roles", label: "Roles", icon: ShieldCheck, permission: PERMISSIONS.roles.read },
];

/** Permissions that open the admin area; holding any one is enough. */
export const ADMIN_PERMISSIONS = [PERMISSIONS.users.read, PERMISSIONS.roles.read];

export function visibleNav(variant: ShellVariant, isAuthenticated: boolean, permissions: string[]) {
  const items = variant === "admin" ? ADMIN_NAV : MAIN_NAV;
  return items.filter(
    (item) => (isAuthenticated || item.public) && (!item.permission || permissions.includes(item.permission)),
  );
}

// trailingSlash export means pathnames arrive as "/todo/"; compare without it.
const normalize = (path: string) => (path.length > 1 ? path.replace(/\/+$/, "") : path);

export function isActive(item: NavItem, pathname: string) {
  const path = normalize(pathname);
  const isSectionRoot = item.href === "/" || item.href === "/admin";
  return path === item.href || (!isSectionRoot && path.startsWith(`${item.href}/`));
}
