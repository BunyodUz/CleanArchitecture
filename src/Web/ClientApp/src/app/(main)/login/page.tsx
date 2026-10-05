import { Suspense } from "react";
import { LoginView } from "@/views/login";

export default function Page() {
  return (
    <Suspense>
      <LoginView />
    </Suspense>
  );
}
