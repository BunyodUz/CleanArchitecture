import { AuthGuard } from "@/features/auth";
import { PERMISSIONS } from "@/shared/config/permissions";
import { RolesView } from "@/views/roles";

export default function Page() {
  return (
    <AuthGuard requirePermissions={[PERMISSIONS.roles.read]}>
      <RolesView />
    </AuthGuard>
  );
}
