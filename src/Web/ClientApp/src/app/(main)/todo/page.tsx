import { AuthGuard } from "@/features/auth";
import { PERMISSIONS } from "@/shared/config/permissions";
import { TodoView } from "@/views/todo";

export default function Page() {
  return (
    <AuthGuard requirePermissions={[PERMISSIONS.todoLists.read]}>
      <TodoView />
    </AuthGuard>
  );
}
