import { AuthGuard } from "@/features/auth";
import { PERMISSIONS } from "@/shared/config/permissions";
import { UsersView } from "@/views/users";

export default function Page() {
  return (
    <AuthGuard requirePermissions={[PERMISSIONS.users.read]}>
      <UsersView />
    </AuthGuard>
  );
}
