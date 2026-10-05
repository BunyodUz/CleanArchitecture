import { Suspense } from "react";
import { AuthGuard } from "@/features/auth";
import { PERMISSIONS } from "@/shared/config/permissions";
import { UsersView } from "@/views/users";

export default function Page() {
  return (
    <AuthGuard requirePermissions={[PERMISSIONS.users.read]}>
      {/* UsersView reads ?search=, which a static export only knows in the browser. */}
      <Suspense>
        <UsersView />
      </Suspense>
    </AuthGuard>
  );
}
