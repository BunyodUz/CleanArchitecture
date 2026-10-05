import { AuthGuard } from "@/features/auth";
import { PERMISSIONS } from "@/shared/config/permissions";
import { AuditView } from "@/views/audit";

export default function Page() {
  return (
    <AuthGuard requirePermissions={[PERMISSIONS.audit.read]}>
      <AuditView />
    </AuthGuard>
  );
}
