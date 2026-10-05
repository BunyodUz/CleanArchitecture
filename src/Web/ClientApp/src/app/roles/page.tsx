import { AuthGuard } from "@/features/auth";
import { PERMISSIONS } from "@/shared/config/permissions";
import { RolesView } from "@/views/roles";

export default function Page() {
  return (
    <AuthGuard requirePermission={PERMISSIONS.roles.read}>
      <RolesView />
    </AuthGuard>
  );
}
