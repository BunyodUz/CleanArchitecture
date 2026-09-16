import { AuthGuard } from "@/features/auth";
import { TodoView } from "@/views/todo";

export default function Page() {
  return (
    <AuthGuard>
      <TodoView />
    </AuthGuard>
  );
}
