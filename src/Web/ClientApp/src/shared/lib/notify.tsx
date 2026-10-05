import { notifications } from "@mantine/notifications";
import { Check, X } from "lucide-react";
import { getValidationErrors } from "./api-error";

export function notifySuccess(title: string, message?: string) {
  notifications.show({ title, message, color: "green", icon: <Check size={18} /> });
}

/** Shows the server's own validation message when there is one, so the user learns what to fix. */
export function notifyError(title: string, error?: unknown) {
  const firstValidationMessage = Object.values(getValidationErrors(error) ?? {})[0]?.[0];
  const status = typeof error === "object" && error !== null && "status" in error ? (error as { status?: number }).status : undefined;

  const message =
    firstValidationMessage ??
    (status === 403
      ? "You don't have permission to do that."
      : status === 401
        ? "Your session has expired. Sign in again to continue."
        : "Something went wrong on the server. Try again in a moment.");

  notifications.show({ title, message, color: "red", icon: <X size={18} />, autoClose: 7000 });
}
