import { AuthGuard } from "@/features/auth";
import { UsersView } from "@/views/users";

export default function Page() {
  return (
    <AuthGuard requireRole="Administrator">
      <UsersView />
    </AuthGuard>
  );
}
