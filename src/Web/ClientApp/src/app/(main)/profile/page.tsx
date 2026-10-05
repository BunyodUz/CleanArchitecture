import { AuthGuard } from "@/features/auth";
import { ProfileView } from "@/views/profile";

export default function Page() {
  return (
    <AuthGuard>
      <ProfileView />
    </AuthGuard>
  );
}
