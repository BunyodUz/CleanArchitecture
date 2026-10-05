"use client";

import { Button, Group, Modal, Text, ThemeIcon } from "@mantine/core";
import { useUnit } from "effector-react";
import { Clock, LogIn } from "lucide-react";
import { usePathname } from "next/navigation";
import { $sessionExpired } from "@/entities/session";
import { login } from "../model";

/** Shown when an API call is rejected because the sign-in session ran out. */
export function SessionExpiredModal() {
  const expired = useUnit($sessionExpired);
  const pathname = usePathname();

  return (
    <Modal
      opened={expired}
      onClose={() => {}}
      withCloseButton={false}
      closeOnClickOutside={false}
      closeOnEscape={false}
      centered
      size="sm"
    >
      <Group align="flex-start" wrap="nowrap" gap="md">
        <ThemeIcon size={40} radius="xl" variant="light" color="orange">
          <Clock size={20} />
        </ThemeIcon>
        <div>
          <Text fw={700} mb={4}>
            Your session has expired
          </Text>
          <Text fz="sm" c="dimmed">
            You were signed out after a period of inactivity. Sign in again to pick up where you left off.
          </Text>
        </div>
      </Group>
      <Group justify="flex-end" mt="lg">
        <Button leftSection={<LogIn size={16} />} onClick={() => login(pathname)}>
          Sign in again
        </Button>
      </Group>
    </Modal>
  );
}
