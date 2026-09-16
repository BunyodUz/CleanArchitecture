"use client";

import { Text } from "@mantine/core";
import { useSearchParams } from "next/navigation";
import { useEffect } from "react";
import { login } from "@/features/auth";

export function LoginView() {
  const searchParams = useSearchParams();

  useEffect(() => {
    login(searchParams.get("returnUrl") ?? "/");
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return <Text>Redirecting to sign in&hellip;</Text>;
}
