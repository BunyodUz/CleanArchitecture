import type { ReactNode } from "react";
import { AppFrame } from "@/widgets/app-shell";

export default function MainLayout({ children }: { children: ReactNode }) {
  return <AppFrame variant="main">{children}</AppFrame>;
}
