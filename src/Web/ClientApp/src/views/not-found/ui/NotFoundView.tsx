"use client";

import { Button } from "@mantine/core";
import { FileQuestion, House } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { StatusPage } from "@/shared/ui/StatusPage";

export function NotFoundView() {
  const pathname = usePathname();

  return (
    <StatusPage
      code="404"
      icon={<FileQuestion size={28} />}
      title="Page not found"
      actions={
        <Button component={Link} href="/" leftSection={<House size={16} />}>
          Back to Home
        </Button>
      }
    >
      There&apos;s nothing at <b>{pathname}</b>. Check the address, or go back to the home page.
    </StatusPage>
  );
}
